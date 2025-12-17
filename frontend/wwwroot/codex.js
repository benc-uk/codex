// options is a JSON string of key-value pairs
export function renderSection(id, desc, optionsJson) {
  const options = JSON.parse(optionsJson);
  console.log("renderSection called from WASM:");
  console.log("id: " + id);
  console.log("desc: " + desc);
  console.log("options:");
  console.dir(options);

  document.querySelector("#main").innerHTML = `
      <h2>${id}</h2>
      <p>${desc}</p>
      <ul>
        ${Object.entries(options)
          .map(([key, value]) => `<li onclick="alert('${key}')">${value}</li>`)
          .join("")}
      </ul>

  `;

  return true;
}
