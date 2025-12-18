export function renderSection(id, text, optionIds, optionTexts) {
  document.getElementById("title").innerText = `Section ${id}`;
  document.getElementById("text").innerText = text;

  const optionsList = document.getElementById("options");
  optionsList.innerHTML = ""; // Clear existing options

  for (let i = 0; i < optionIds.length; i++) {
    const li = document.createElement("li");
    li.onclick = () =>
      globalThis.wasmExports.WebRunner.TakeOption(optionIds[i]);
    li.innerText = optionTexts[i];

    optionsList.appendChild(li);
  }
}
