# Readiness core

The implemented `TypeInformation` evaluator checks required text against normalized wall/ceiling type observations. It returns one finding per missing type field, distinct affected instances, explicit optional fields, and unexamined cases. A type can have both a known missing field and an unexamined field; affected and unexamined instance lists may overlap, so do not add their lengths to obtain a unique total.

The net48 project has no Revit references or NuGet packages. `Checklist` validates required identities and explicit requirement flags. `JsonFile` uses the framework serializer. Run `scripts/Test-Readiness.ps1` from the project directory for 18 behavior tests, including the six-type demonstration and incomplete-coverage cases. See the [contract](../../docs/required-type-information.md) and [lesson](../../../../docs/tutorials/03-check-required-type-information.md). Quantities and physical-condition rules remain future work.
