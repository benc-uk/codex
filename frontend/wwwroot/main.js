import { dotnet } from "./_framework/dotnet.js";
import { renderSection, notify } from "./codex.js";

const { setModuleImports, getAssemblyExports, getConfig, runMain } =
  await dotnet
    .withDiagnosticTracing(false)
    .withApplicationArguments(window.location.href + "stories/cave.yaml")
    .create();

setModuleImports("codex", {
  renderSection,
  notify,
});

// Expose WASM exports globally so codex.js can call them
globalThis.wasmExports = await getAssemblyExports(getConfig().mainAssemblyName);

await runMain();
