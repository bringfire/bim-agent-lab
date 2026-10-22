"""An observer must not remove a control it failed to create exclusively."""
import importlib.util
from pathlib import Path
from types import SimpleNamespace
import tempfile
import unittest
from unittest.mock import patch

FILE=Path(__file__).parent/'host-diagnostics/Observe-LiveDeadline.py'
spec=importlib.util.spec_from_file_location('host_observer',FILE)
observer=importlib.util.module_from_spec(spec); spec.loader.exec_module(observer)


class ObserverOwnershipTests(unittest.TestCase):
    def test_competing_arm_is_preserved_when_exclusive_create_fails(self):
        with tempfile.TemporaryDirectory() as tmp:
            root=Path(tmp)/'host'; root.mkdir()
            (root/'events.tsv').write_text('ApplicationStartup\nEventsHooked\n')
            arm=root/'arm-Preparation.txt'
            args=SimpleNamespace(trace_directory=str(root),evidence=str(Path(tmp)/'capture'),
                                 stage='Preparation',origin='deadline',revit_pid=123,client='unused',server='unused')
            real_open=Path.open
            def competing_open(path,mode='r',*a,**kw):
                if path==arm and mode=='x':
                    with real_open(arm,'w') as out: out.write('other operator control')
                    raise FileExistsError(str(arm))
                return real_open(path,mode,*a,**kw)
            with patch.object(Path,'open',competing_open):
                with self.assertRaises(FileExistsError): observer.run(args)
            self.assertTrue(arm.exists(),'Cleanup removed a control it did not own')
            self.assertEqual(arm.read_text(),'other operator control')


if __name__=='__main__': unittest.main()
