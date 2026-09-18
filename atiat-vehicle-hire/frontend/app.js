const API = window.ATIAT_API_URL.replace(/\/$/, '');
const form = document.getElementById('hireForm');
const successBox = document.getElementById('successBox');
const errorBox = document.getElementById('errorBox');
const dateInput = document.getElementById('hireDate');

const today = new Date();
dateInput.min = new Date(today.getTime() - today.getTimezoneOffset()*60000).toISOString().split('T')[0];

form.addEventListener('submit', async (e) => {
  e.preventDefault();
  successBox.classList.add('hidden');
  errorBox.classList.add('hidden');
  const data = Object.fromEntries(new FormData(form).entries());
  data.passengers = Number(data.passengers || 1);
  data.driverRequired = data.driverRequired === 'true';
  data.hireDate = new Date(`${data.hireDate}T00:00:00`).toISOString();

  const button = form.querySelector('button');
  button.disabled = true; button.textContent = 'Submitting...';

  try {
    const response = await fetch(`${API}/api/requests`, {
      method: 'POST', headers: {'Content-Type':'application/json'}, body: JSON.stringify(data)
    });
    const result = await response.json();
    if (!response.ok) throw new Error(result.message || 'Unable to submit request.');
    form.reset();
    dateInput.min = new Date().toISOString().split('T')[0];
    successBox.innerHTML = `<h3>Request received successfully.</h3><p>Your request has been sent to ATIAT for review.</p><p>Your reference number is:</p><div class="reference">${result.referenceNumber}</div><p>Keep this reference number for follow-up on WhatsApp or with an ATIAT representative.</p>`;
    successBox.classList.remove('hidden');
    successBox.scrollIntoView({behavior:'smooth',block:'center'});
  } catch (error) {
    errorBox.textContent = error.message;
    errorBox.classList.remove('hidden');
  } finally {
    button.disabled = false; button.textContent = 'Submit Vehicle Hire Request';
  }
});
