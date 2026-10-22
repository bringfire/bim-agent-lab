"""Capture with the existing official-SDK driver, then independently verify.

Verification orchestration only. No MCP implementation, Revit API access,
model editing, installation or desktop registration lives here.
"""
import argparse
from datetime import datetime, timezone
import hashlib
import json
import math
from pathlib import Path, PureWindowsPath
import re
import subprocess
import sys
import time
import uuid

PROJECT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(PROJECT / 'tests'))
from verify_mcp_evidence import CASES, exact, expected_report, read, verify

PROTOCOLS = ('2025-06-18', '2025-11-25', '2026-07-28')


def utc():
    return datetime.now(timezone.utc)


def timestamp(value):
    result = datetime.fromisoformat(value.replace('Z', '+00:00'))
    if result.tzinfo is None or result.utcoffset().total_seconds() != 0:
        raise ValueError('Evidence timestamps must be UTC.')
    return result


def sha(path):
    digest = hashlib.sha256()
    with Path(path).open('rb') as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b''):
            digest.update(block)
    return digest.hexdigest()


def create_bytes(path, data):
    # Never replace evidence. An interrupted write is retained without a success marker.
    import os
    with Path(path).open('xb') as stream:
        stream.write(data)
        stream.flush()
        os.fsync(stream.fileno())


def create_json(path, value):
    create_bytes(path, (json.dumps(value, indent=2, ensure_ascii=True) + '\n').encode('utf-8'))


def inputs(prepared, expectation, client, server):
    records = []
    for kind in ('model', 'checklist'):
        path = Path(prepared[kind + 'Path'])
        if not path.is_absolute() or sha(path) != prepared[kind + 'Sha256']:
            raise ValueError(f'{kind} bytes differ from the independently prepared expectation: {path}')
        records.append(dict(path=str(path), sha256=prepared[kind + 'Sha256']))
    for item in prepared.get('oracleSources', []):
        path = Path(item['path'])
        if not path.is_absolute(): path = PROJECT.parents[1] / path
        if sha(path) != item['sha256']:
            raise ValueError(f'Independent source changed: {path}')
        records.append(dict(path=str(path.resolve()), sha256=item['sha256']))
    if prepared['case'] == 'nc1':
        if prepared.get('absenceConfirmed') is not True:
            raise ValueError('NC1 needs an independent absent-ID observation before context acquisition.')
        proof = prepared['absenceEvidence']
        path = Path(proof['path'])
        if not path.is_absolute() or sha(path) != proof['sha256']:
            raise ValueError('NC1 independent observation is missing or changed.')
        records.append(dict(path=str(path), sha256=proof['sha256']))
    for path in [expectation, server, *client]:
        path = Path(path).resolve(strict=True)
        records.append(dict(path=str(path), sha256=sha(path)))
        # Apphost bytes alone do not identify the managed application.
        companion = path.with_suffix('.dll')
        if path.suffix.lower() == '.exe' and companion.exists():
            records.append(dict(path=str(companion), sha256=sha(companion)))
    return records


def unchanged(records):
    for record in records:
        if sha(record['path']) != record['sha256']:
            raise ValueError('Input changed during capture: ' + record['path'])


def guard_context(result, prepared):
    if result.get('isError') is not False:
        raise ValueError('The SDK could not acquire a successful readiness context.')
    ctx = result['structuredContent']
    if len(result['content']) != 1 or json.loads(result['content'][0]['text']) != ctx:
        raise ValueError('Context text and structured result differ.')
    for key in ('revitPid', 'bridgeSessionId', 'checklistSha256'):
        if ctx[key] != prepared[key]: raise ValueError('Context does not match expectation: ' + key)
    for actual, wanted in [('documentPath', 'modelPath'), ('checklistPath', 'checklistPath')]:
        if PureWindowsPath(ctx[actual]) != PureWindowsPath(prepared[wanted]):
            raise ValueError('The active document/checklist is not the prepared case: ' + actual)
    for actual, wanted in [('projectUniqueId', 'ProjectUniqueId'), ('revitBuild', 'RevitBuild'),
            ('checklistId', 'ChecklistId'), ('scope', 'Scope'), ('level', 'Level'), ('phase', 'Phase')]:
        if ctx[actual] != prepared['expectedReport'][wanted]:
            raise ValueError('Context disagrees with the independent expectation: ' + actual)
    if ctx['contractVersion'] != 1 or not re.fullmatch('[0-9a-f]{32}', ctx['requestId']):
        raise ValueError('Invalid context response identity.')
    if not isinstance(ctx['contextToken'], str) or not ctx['contextToken'] or timestamp(ctx['expiresUtc']) <= utc():
        raise ValueError('Context is absent or already expired.')
    return ctx


def retire(process):
    if process is None or process.poll() is not None: return
    # This process belongs to this invocation. Never terminate Revit or a desktop.
    # Its server sees stdio EOF; production transport owns request cancellation.
    try: process.terminate()
    except OSError:
        if process.poll() is None: raise
    try: process.wait(timeout=2)
    except subprocess.TimeoutExpired:
        process.kill()
        process.wait(timeout=2)


def run(*, expectation, output, server, client, case, revit_pid,
        protocol='2026-07-28', timeout_seconds=450):
    if not __debug__:
        raise ValueError('Run without Python -O/PYTHONOPTIMIZE; independent verification uses assertions.')
    expectation, output, server = Path(expectation).resolve(), Path(output).resolve(), Path(server).resolve()
    if output.exists(): raise FileExistsError('Use a NEW evidence directory; prior attempts are never overwritten.')
    client = [str(Path(item).resolve(strict=True)) for item in client]
    if case not in CASES or protocol not in PROTOCOLS or type(revit_pid) is not int or revit_pid <= 0:
        raise ValueError('Select an approved case/protocol and a positive current Revit PID.')
    if not math.isfinite(timeout_seconds) or timeout_seconds <= 0 or timeout_seconds > 600:
        raise ValueError('The entire capture budget must be positive and no more than 600 seconds.')
    raw = expectation.read_bytes()
    prepared = json.loads(raw.decode('utf-8-sig'))
    if prepared['case'] != case or prepared['revitPid'] != revit_pid:
        raise ValueError('Case/PID disagrees with the prepared expectation; prepare the current session first.')
    if not re.fullmatch('[0-9a-f]{32}', prepared['bridgeSessionId']):
        raise ValueError('A prepared bridge session identity is required.')
    if timestamp(prepared['preparedUtc']) > utc(): raise ValueError('Expectation preparation is in the future.')
    if exact(prepared['expectedReport']) != exact(expected_report(case, prepared['modelPath'], prepared['checklistPath'])):
        raise ValueError('Prepared report differs from the independent BIM oracle.')
    records = inputs(prepared, expectation, client, server)
    # Recheck the exact bytes decoded above before committing the prepared record.
    if sha(expectation) != hashlib.sha256(raw).hexdigest(): raise ValueError('Expectation changed during preparation.')
    output.mkdir(parents=True, exist_ok=False)
    capture = output / 'capture'
    release = output / 'release-context'
    command = [*client, '--scenario', 'readiness', '--server', str(server), '--revit-pid', str(revit_pid),
               '--expected', 'ReadinessReport', '--protocol', protocol, '--evidence', str(capture),
               '--pause-file', str(release)]
    process = None
    stage = 'prepare'
    started = utc()
    deadline = time.monotonic() + timeout_seconds  # Never restarted at a later stage.
    try:
        create_bytes(output / 'expectation.json', raw)
        create_json(output / 'prepared.json', dict(runId=uuid.uuid4().hex, startedUtc=started.isoformat(),
            budgetSeconds=timeout_seconds, case=case, protocolVersion=protocol, revitPid=revit_pid,
            bridgeSessionId=prepared['bridgeSessionId'], command=command, files=records,
            kind='SDK capture; not an actual desktop acceptance run'))
        stage = 'SDK context acquisition'
        with (output / 'sdk-stdout.txt').open('xb') as stdout, (output / 'sdk-stderr.txt').open('xb') as stderr:
            unchanged(records)
            process = subprocess.Popen(command, stdin=subprocess.DEVNULL, stdout=stdout, stderr=stderr)
            while not (capture / 'context-ready.json').exists():
                if process.poll() is not None:
                    raise RuntimeError(f'SDK stopped before context validation (exit {process.returncode}); inspect retained logs.')
                if time.monotonic() >= deadline: raise TimeoutError('Capture budget expired while acquiring context.')
                time.sleep(0.025)
            stage = 'context validation before readiness read'
            ctx = guard_context(read(capture / 'context.json'), prepared)
            unchanged(records)
            create_json(output / 'context-validated.json', dict(validatedUtc=utc().isoformat(), requestId=ctx['requestId']))
            if time.monotonic() >= deadline: raise TimeoutError('Capture budget expired before readiness read.')
            create_bytes(release, b'Validated prepared context; SDK may request the read.\n')
            stage = 'SDK read and transport shutdown'
            process.wait(timeout=max(0, deadline - time.monotonic()))
        if process.returncode != 0: raise RuntimeError(f'SDK capture failed (exit {process.returncode}).')
        stage = 'independent verification'
        if read(capture / 'complete.json').get('pass') is not True:
            raise ValueError('SDK completion did not pass.')
        sdk = read(capture / 'sdk-client-result.json')
        if sdk.get('pass') is not True or sdk.get('protocol') != protocol:
            raise ValueError('SDK did not complete the selected protocol successfully.')
        unchanged(records)
        if (output / 'expectation.json').read_bytes() != raw: raise ValueError('Frozen expectation was changed.')
        create_bytes(capture / 'expectation.json', raw)
        check = read(capture / 'check.json')
        files = [dict(path='wire/' + name, bytes=(capture / 'wire' / name).stat().st_size,
                      sha256=sha(capture / 'wire' / name)) for name in ('client-to-server.jsonl', 'server-to-client.jsonl')]
        create_json(capture / 'capture.json', dict(sourceSession='SDK client dedicated process',
            snapshotUtc=utc().isoformat(), snapshotKind='Completed SDK process', protocolVersion=protocol,
            contextRequestId=ctx['requestId'], reportRequestId=check['structuredContent']['requestId'], files=files))
        result = verify(capture, case)
        if not started <= timestamp(check['structuredContent']['report']['GeneratedUtc']) <= utc():
            raise ValueError('Report timestamp falls outside this fresh invocation.')
        unchanged(records)
        if time.monotonic() >= deadline:
            raise TimeoutError('Original capture budget expired before comparison publication.')
        create_json(capture / 'comparison-complete.json', dict(verifiedUtc=utc().isoformat(), **result))
        return result
    except BaseException as error:
        cleanup_error = None
        try: retire(process)
        except Exception as cleanup:
            cleanup_error = f'{type(cleanup).__name__}: {cleanup}'
        create_json(output / 'failed.json', dict(failedUtc=utc().isoformat(), stage=stage,
            errorType=type(error).__name__, message=str(error), cleanupError=cleanup_error,
            sdkExitCode=None if process is None else process.returncode))
        raise
    finally:
        if process is not None and process.poll() is None: retire(process)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--expectation', required=True)
    parser.add_argument('--output', required=True)
    parser.add_argument('--server', required=True)
    parser.add_argument('--client', required=True)
    parser.add_argument('--case', choices=CASES, required=True)
    parser.add_argument('--revit-pid', type=int, required=True)
    parser.add_argument('--protocol', choices=PROTOCOLS, default='2026-07-28')
    parser.add_argument('--timeout-seconds', type=float, default=450)
    args = parser.parse_args()
    try:
        result = run(expectation=args.expectation, output=args.output, server=args.server, client=[args.client],
                     case=args.case, revit_pid=args.revit_pid, protocol=args.protocol, timeout_seconds=args.timeout_seconds)
        print('PASS independent case comparison: ' + json.dumps(result))
        print('Evidence: ' + str(Path(args.output).resolve() / 'capture'))
        return 0
    except (Exception, KeyboardInterrupt) as error:
        print('FAIL readiness capture: ' + str(error), file=sys.stderr)
        print('No successful comparison was published. Preserve any attempt directory; retry with a new directory.', file=sys.stderr)
        return 1


if __name__ == '__main__': sys.exit(main())
