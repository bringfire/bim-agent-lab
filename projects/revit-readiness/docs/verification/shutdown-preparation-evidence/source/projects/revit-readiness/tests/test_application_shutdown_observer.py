"""Ownership checks; these do not simulate or establish an Autodesk shutdown."""
import importlib.util
from pathlib import Path
import tempfile
import unittest
from unittest.mock import Mock, patch
import json

spec=importlib.util.spec_from_file_location('shutdown_observer',Path(__file__).parent/'host-diagnostics/Observe-ApplicationShutdown.py')
observer=importlib.util.module_from_spec(spec); spec.loader.exec_module(observer)


class ShutdownOwnershipTests(unittest.TestCase):
    def test_cleanup_error_does_not_skip_child_or_evidence(self):
        with tempfile.TemporaryDirectory() as temp:
            root=Path(temp); evidence=root/'evidence'; evidence.mkdir()
            (root/'events.tsv').write_text('retained trace')
            child=Mock(); child.poll.return_value=0
            record={'captureCompleted':False,'error':'original failure'}
            with patch.object(observer,'cleanup_arms',side_effect=PermissionError('locked')):
                errors=observer.finish_capture(root,evidence,[],[child],record)
            self.assertEqual(len(errors),1); child.wait.assert_called_once_with(timeout=15)
            self.assertTrue((evidence/'finish.signal').exists())
            self.assertEqual((evidence/'after.tsv').read_text(),'retained trace')
            saved=json.loads((evidence/'observation.json').read_text())
            self.assertEqual(saved['error'],'original failure'); self.assertFalse(saved['captureCompleted'])
            self.assertIn('PermissionError',saved['cleanupErrors'][0])

    def test_only_owned_lifecycle_controls_are_cleaned_up(self):
        for mode in ('unused','consumed','foreign-arm','foreign-used','released'):
            with self.subTest(mode=mode),tempfile.TemporaryDirectory() as temp:
                root=Path(temp); arm=root/'arm-ApplicationClosing.txt'; used=root/'used-ApplicationClosing.txt'; release=root/'release-ApplicationClosing.signal'
                arm.write_text('45000'); stat=arm.stat(); owned=[('ApplicationClosing',(stat.st_dev,stat.st_ino))]
                if mode in ('consumed','foreign-arm','released'): arm.rename(used)
                if mode=='foreign-arm': arm.write_text('foreign')
                if mode=='foreign-used': used.write_text('foreign')
                if mode=='released': release.write_text('explicit release')
                result=observer.cleanup_arms(root,owned)
                self.assertEqual(result,mode in ('consumed','foreign-arm'))
                if mode=='foreign-arm': self.assertEqual(arm.read_text(),'foreign')
                else: self.assertFalse(arm.exists())
                if mode=='foreign-used': self.assertEqual(used.read_text(),'foreign'); self.assertFalse(release.exists())
                if mode=='released': self.assertEqual(release.read_text(),'explicit release')
                if mode in ('consumed','foreign-arm'): self.assertEqual(release.read_text(),'observer cleanup')


if __name__=='__main__': unittest.main()
