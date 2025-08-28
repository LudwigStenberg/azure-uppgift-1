const form = document.getElementById("registration-form");

form.addEventListener("submit", async (event) => {
  event.preventDefault();
  console.log("Form submitted");

  const formData = new FormData(form);
  const firstName = formData.get("firstName");
  console.log("First name:", firstName);
  registerVisit(firstName);
});

async function registerVisit(firstName) {
  const url = "http://localhost:7071/api/RegisterVisitor";

  try {
    const response = await fetch(url, {
      method: "POST",
      body: JSON.stringify({ firstName: firstName }),
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) {
      throw new Error(`Error: ${response.status}`);
    }

    const result = await response.json();
    console.log(result);
  } catch (error) {
    console.error(error.message);
  }
}
