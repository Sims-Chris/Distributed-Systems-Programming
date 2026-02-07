using PipesAndFilters;

ServerEnvironment.Setup();
Client client = new Client();
client.RequestHello("HELLO WORLD!");