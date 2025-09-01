const form = document.getElementById("registration-form");
const responseMessage = document.getElementById("response-message");

form.addEventListener("submit", async (event) => {
  event.preventDefault();
  console.log("Form submitted");

  const formData = new FormData(form);
  const firstName = formData.get("firstname");
  const lastName = formData.get("lastname");
  const emailAddress = formData.get("email");
  console.log(
    `First name: ${firstName}, Last name: ${lastName}, Email: ${emailAddress}`
  );
  registerVisit(firstName, lastName, emailAddress);

  document.getElementById("input-firstname").value = "";
  document.getElementById("input-lastname").value = "";
  document.getElementById("input-email").value = "";
});

async function registerVisit(firstName, lastName, emailAddress) {
  const localUrl = "http://localhost:7071/api/RegisterVisitor";
  const publicUrl =
    "https://func-uppgift1.azurewebsites.net/api/RegisterVisitor";

  try {
    const response = await fetch(publicUrl, {
      method: "POST",
      body: JSON.stringify({
        firstName: firstName,
        lastName: lastName,
        emailAddress: emailAddress,
      }),
      headers: {
        "Content-Type": "application/json",
      },
    });

    if (!response.ok) {
      throw new Error(`Error: ${response.status}`);
    }

    const result = await response.json();
    console.log(result);

    const utcTimestamp = result.timestamp + "Z";
    const localDate = new Date(utcTimestamp);

    responseMessage.innerText = `Welcome, ${result.firstName} ${
      result.lastName
    }.\nYou checked in at: ${localDate.toLocaleString(undefined, {
      year: "numeric",
      month: "short",
      day: "numeric",
      hour: "numeric",
      minute: "2-digit",
    })}`;
    setTimeout(() => {
      document.getElementById("response-message").style.display = "none";
    }, 7000);
  } catch (error) {
    console.error(error.message);
    responseMessage.innerText = `There was an error registering your visit. Please try again.`;
  }
}
