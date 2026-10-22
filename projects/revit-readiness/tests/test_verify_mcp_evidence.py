"""Actual native-command principal fixtures plus specification-authored NC fixtures."""
import copy
import json
import unittest
from verify_mcp_evidence import ABSENT_ID, EVIDENCE, verify_report


def fixture(case):
    stem = 'BAL-info-' + (case if case in ('complete','wall-defects','ceiling-negative') else 'complete')
    report = json.loads((EVIDENCE/(stem+'.readiness.json')).read_text(encoding='utf-8-sig'))
    if case == 'nc1':
        report.update(ExaminedInstances=15, OtherHostWallCeilingInstances=1197, CoverageComplete=False)
        report['ScopeIssues'] = [{'CaseId':'BAL-INFO-WA-01','UniqueId':ABSENT_ID,'Status':'Unexamined','Reason':'Element not found'}]
        wall = next(t for t in report['Evaluation']['Types'] if t['TypeName']=='BAL INFO Wall A')
        wall['Elements'] = [e for e in wall['Elements'] if e['CaseId']!='BAL-INFO-WA-01']
    if case == 'nc2':
        report['CoverageComplete'] = report['Evaluation']['Complete'] = False
        act = next(t for t in report['Evaluation']['Types'] if t['TypeName']=='BAL INFO Ceiling ACT')
        act['Status'] = 'Unexamined'
        report['Evaluation']['UnexaminedElements'] = copy.deepcopy(act['Elements'])
        for f in act['Fields']:
            f.update(Status='Unexamined',Reason='NoMatchingRequirement',Required=None,RequirementSource=None)
    return report


class OracleTests(unittest.TestCase):
    def verify(self, report, case):
        return verify_report(report,case,report['DocumentPath'],report['ChecklistPath'])

    def test_five_expected_cases(self):
        for case, findings, affected, examined, flags in [
            ('complete',0,0,16,(True,True)),('wall-defects',2,6,16,(True,True)),
            ('ceiling-negative',3,8,16,(True,True)),('nc1',0,0,15,(False,True)),('nc2',0,0,16,(False,False))]:
            with self.subTest(case=case):
                r=self.verify(fixture(case),case)
                self.assertIn('findings',r)
                self.assertEqual((r['findings'],r['affectedInstances'],r['examinedInstances']), (findings,affected,examined))
                self.assertEqual((r['coverageComplete'],r['evaluationComplete']),flags)

    def test_corruption_matrix(self):
        changes = [
            ('unit',lambda r:r.update(Units='square feet')),
            ('scope',lambda r:r.update(Scope='all visible elements')),
            ('phase',lambda r:r.update(Phase='Existing')),
            ('project',lambda r:r.update(ProjectUniqueId='wrong')),
            ('extra key',lambda r:r.update(Truncated=True)),
            ('other hosts',lambda r:r.update(OtherHostWallCeilingInstances=0)),
            ('links',lambda r:r.update(LinkInstancesExcluded=0)),
            ('modified',lambda r:r.update(ModifiedAfter=True)),
            ('timestamp',lambda r:r.update(GeneratedUtc='not a date')),
            ('missing type',lambda r:r['Evaluation']['Types'].pop()),
            ('type id',lambda r:r['Evaluation']['Types'][0].update(TypeUniqueId='wrong')),
            ('type status',lambda r:r['Evaluation']['Types'][0].update(Status='Invented')),
            ('missing element',lambda r:r['Evaluation']['Types'][0]['Elements'].pop()),
            ('element id',lambda r:r['Evaluation']['Types'][0]['Elements'][0].update(ElementId='42')),
            ('element uid',lambda r:r['Evaluation']['Types'][0]['Elements'][0].update(UniqueId='wrong')),
            ('case id',lambda r:r['Evaluation']['Types'][0]['Elements'][0].update(CaseId='wrong')),
            ('missing field',lambda r:r['Evaluation']['Types'][0]['Fields'].pop()),
            ('parameter',lambda r:r['Evaluation']['Types'][0]['Fields'][0].update(ParameterId='wrong')),
            ('rule',lambda r:r['Evaluation']['Types'][0]['Fields'][0].update(RuleId='wrong')),
            ('source',lambda r:r['Evaluation']['Types'][0]['Fields'][0].update(RequirementSource='wrong')),
            ('raw value',lambda r:r['Evaluation']['Types'][0]['Fields'][0].update(ObservedValue='invented')),
            ('required',lambda r:r['Evaluation']['Types'][0]['Fields'][0].update(Required='yes')),
            ('reason',lambda r:r['Evaluation']['Types'][0]['Fields'][0].update(Reason='invented')),
            ('coverage absent',lambda r:r.pop('CoverageComplete')),
            ('completeness absent',lambda r:r['Evaluation'].pop('Complete')),
            ('coverage inverted',lambda r:r.update(CoverageComplete=not r['CoverageComplete'])),
            ('complete inverted',lambda r:r['Evaluation'].update(Complete=not r['Evaluation']['Complete'])),
        ]
        for case in ('complete','wall-defects','ceiling-negative','nc1','nc2'):
            for label,change in changes:
                with self.subTest(case=case,change=label):
                    report=fixture(case);change(report)
                    with self.assertRaises((AssertionError,ValueError,KeyError)):self.verify(report,case)

    def test_control_identities_and_raw_nulls(self):
        cases = [
            ('nc1', lambda r:r.update(ScopeIssues=[])),
            ('nc1', lambda r:r['ScopeIssues'][0].update(UniqueId='wrong')),
            ('nc1', lambda r:r['ScopeIssues'][0].update(Reason='Wrong reason')),
            ('nc2', lambda r:r['Evaluation']['UnexaminedElements'].pop()),
            ('nc2', lambda r:r['Evaluation']['UnexaminedElements'][0].update(CaseId='wrong')),
            ('complete', lambda r:r['Evaluation']['Types'][0]['Fields'][1].update(ObservedValue='')),
            ('wall-defects', lambda r:r['Evaluation']['AffectedElements'].pop()),
            ('ceiling-negative', lambda r:r['Evaluation'].update(FindingCount=2)),
            ('complete', lambda r:r.update(CoverageComplete=1)),
        ]
        for case,change in cases:
            with self.subTest(case=case,change=change):
                report=fixture(case);change(report)
                with self.assertRaises((AssertionError,ValueError,KeyError)):self.verify(report,case)

    def test_paths_and_unknown_case(self):
        report=fixture('complete')
        for case,model,checklist in [('wrong',report['DocumentPath'],report['ChecklistPath']),
                                     ('complete','D:/wrong.rvt',report['ChecklistPath']),
                                     ('complete',report['DocumentPath'],'D:/wrong.checklist.json')]:
            with self.assertRaises((AssertionError,ValueError,KeyError)):
                verify_report(report,case,model,checklist)


if __name__=='__main__':unittest.main()
