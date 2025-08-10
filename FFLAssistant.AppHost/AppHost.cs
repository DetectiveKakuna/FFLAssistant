var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.FFLAssistant>("fflassistant");

builder.Build().Run();