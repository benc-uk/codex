import { dotnet } from "./_framework/dotnet.js";
import { renderSection } from "./codex.js";

const { setModuleImports, getAssemblyExports, getConfig, runMain } =
  await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArgumentsFromQuery()
    .create();

setModuleImports("codex", {
  renderSection,
});

const config = getConfig();
const exports = await getAssemblyExports(config.mainAssemblyName);

// Expose WASM exports globally so codex.js can call them
globalThis.wasmExports = exports;
// Also expose takeOption globally for onclick handlers
// globalThis.takeOption = takeOption;

await runMain();
