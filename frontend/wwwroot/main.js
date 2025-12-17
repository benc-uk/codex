import { dotnet } from "./_framework/dotnet.js";
import { renderSection } from "./codex.js";

const { setModuleImports, getAssemblyExports, getConfig } = await dotnet
  .withDiagnosticTracing(false)
  .withApplicationArgumentsFromQuery()
  .create();

setModuleImports("codex", {
  renderSection,
});

const config = getConfig();
// const exports = await getAssemblyExports(config.mainAssemblyName);
await dotnet.run();
console.log("END");
