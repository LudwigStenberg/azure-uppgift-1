const form = document.getElementById("registration-form");
const responseMessage = document.getElementById("response-message");

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
    console.log(result.timestamp);

    const utcTimestamp = result.timestamp + "Z";
    const localDate = new Date(utcTimestamp);

    responseMessage.innerText = `Welcome, ${
      result.firstName
    }.\nYou checked in at: ${localDate.toLocaleString(undefined, {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "numeric",
      minute: "2-digit",
    })}`;
  } catch (error) {
    console.error(error.message);
    responseMessage.innerText = `There was an error registering your visit. Please try again.`;
  }
}
