"""Real installed API/build selection checks; no claim of live Revit execution."""
import json
from pathlib import Path
import subprocess
import unittest

PROJECT = Path(__file__).resolve().parents[1]


class RevitTargetTests(unittest.TestCase):
    def environment(self, *args):
        return subprocess.run(['pwsh', '-NoProfile', '-File', str(PROJECT / 'scripts/Check-Environment.ps1'), *args], cwd=PROJECT, capture_output=True, text=True)

    def properties(self, name, year):
        path = PROJECT / ('tests/fixture-tools' if name == 'BimAgentLab.FixtureTools' else 'src/revit-addin') / (name + '.csproj')
        result = subprocess.run(['dotnet', 'msbuild', str(path), '-nologo', f'-p:RevitYear={year}', '-getProperty:TargetFramework,BaseOutputPath,BaseIntermediateOutputPath,RevitInstallDir,NuGetLockFilePath'], cwd=PROJECT, capture_output=True, text=True)
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        return json.loads(result.stdout)['Properties']

    def test_default_environment_retains_2024(self):
        result = self.environment()
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        self.assertEqual('2024', json.loads(result.stdout)['revitYear'])
        self.assertEqual('net48', json.loads(result.stdout)['hostTargetFramework'])

    def test_installed_2026_5_environment_selects_modern_runtime(self):
        result = self.environment('-RevitYear', '2026')
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)
        report = json.loads(result.stdout)
        self.assertEqual('2026', report['revitYear'])
        self.assertEqual('net10.0-windows', report['hostTargetFramework'])
        self.assertEqual('net10.0', report['revitRuntimeFramework'])

    def test_wrong_year_api_is_rejected(self):
        result = self.environment('-RevitYear', '2026', '-RevitInstallDir', r'C:\Program Files\Autodesk\Revit 2024')
        self.assertNotEqual(0, result.returncode)
        self.assertIn('RevitInstallDir must point to Revit 2026', result.stdout)

    def test_direct_msbuild_rejects_wrong_year_binding(self):
        result = subprocess.run(['dotnet', 'msbuild', str(PROJECT / 'src/revit-addin/BimAgentLab.RevitProbe.csproj'), '-nologo', '-t:VerifyRevitHostBinding', '-p:RevitYear=2026', r'-p:RevitInstallDir=C:\Program Files\Autodesk\Revit 2024'], cwd=PROJECT, capture_output=True, text=True)
        self.assertNotEqual(0, result.returncode)
        self.assertIn('Revit API year does not match', result.stdout + result.stderr)

    def test_three_host_projects_have_distinct_modern_intermediates_and_legacy_outputs(self):
        modern_intermediates = []
        modern_locks = []
        for name in ('BimAgentLab.RevitProbe', 'BimAgentLab.RevitReadiness', 'BimAgentLab.FixtureTools'):
            with self.subTest(project=name):
                legacy = self.properties(name, '2024')
                modern = self.properties(name, '2026')
                self.assertEqual('net48', legacy['TargetFramework'])
                self.assertEqual('net10.0-windows', modern['TargetFramework'])
                self.assertNotIn('Revit2026', legacy['BaseOutputPath'])
                self.assertIn('Revit2026', modern['BaseOutputPath'])
                self.assertIn('Revit2026', modern['BaseIntermediateOutputPath'])
                self.assertIn(name, modern['BaseIntermediateOutputPath'])
                self.assertIn(name, Path(modern['NuGetLockFilePath']).name)
                self.assertIn('Revit 2026', modern['RevitInstallDir'])
                modern_intermediates.append(str(Path(modern['RevitInstallDir']).parent / name / modern['BaseIntermediateOutputPath']))
                modern_locks.append(modern['NuGetLockFilePath'])
        self.assertEqual(3, len(set(modern_intermediates)))
        self.assertEqual(3, len(set(modern_locks)))


if __name__ == '__main__':
    unittest.main()
