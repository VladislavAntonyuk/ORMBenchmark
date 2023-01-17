# ORM_tests

1. Setup connection string in App.config
1. Build application in release mode

## Add new ORMs

1. Implement `IOperation` interface
1. Override `ToString()` in your implementation
1. Add your implementation instance to the `Operations` enumerable in `RunBenchmark` class