"""Cleanup must use file identity, never assume a same-named arm is ours."""
import importlib.util
from pathlib import Path
import tempfile
import unittest

spec = importlib.util.spec_from_file_location('queued_stop', Path(__file__).parent/'host-diagnostics/Observe-QueuedStop.py')
observer = importlib.util.module_from_spec(spec)
spec.loader.exec_module(observer)


class OwnedArmsTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)
        self.arm = self.root/'arm-Reader.txt'
        self.arm.write_text('45000\nstop\n')
        s = self.arm.stat()
        self.owned = [('Reader', self.arm, (s.st_dev, s.st_ino))]

    def test_unconsumed_owned_arm_removed(self):
        observer.cleanup_arms(self.root, self.owned)
        self.assertFalse(self.arm.exists())

    def test_consumed_arm_released(self):
        self.arm.rename(self.root/'used-abc-Reader.txt')
        self.assertTrue(observer.cleanup_arms(self.root, self.owned))
        self.assertTrue((self.root/'release-abc-Reader.signal').exists())

    def test_foreign_arm_preserved_after_claim(self):
        self.arm.rename(self.root/'used-abc-Reader.txt')
        self.arm.write_text('foreign')
        observer.cleanup_arms(self.root, self.owned)
        self.assertEqual('foreign', self.arm.read_text())
        self.assertTrue((self.root/'release-abc-Reader.signal').exists())

    def test_foreign_claim_not_released(self):
        (self.root/'used-other-Reader.txt').write_text('45000\nstop\n')
        observer.cleanup_arms(self.root, self.owned)
        self.assertFalse((self.root/'release-other-Reader.signal').exists())
        self.assertFalse(self.arm.exists())

    def test_existing_release_is_unchanged(self):
        self.arm.rename(self.root/'used-abc-Reader.txt')
        release = self.root/'release-abc-Reader.signal'
        release.write_text('explicit release')
        self.assertFalse(observer.cleanup_arms(self.root, self.owned))
        self.assertEqual('explicit release', release.read_text())


if __name__=='__main__':
    unittest.main()
