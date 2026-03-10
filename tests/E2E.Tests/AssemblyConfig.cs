// Disable parallel execution across all e2e test collections.
// Each test class boots real HTTP servers + uses shared RabbitMQ queues,
// so parallel runs would cause messages to be consumed by the wrong test instance.
using Xunit;
[assembly: CollectionBehavior(DisableTestParallelization = true)]
