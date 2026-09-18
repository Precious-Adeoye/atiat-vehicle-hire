const API = window.ATIAT_API_URL.replace(/\/$/, '');
let adminKey = '';
const loginCard = document.getElementById('loginCard');
const dashboard = document.getElementById('dashboard');

document.getElementById('loginBtn').addEventListener('click', () => {
  adminKey = document.getElementById('adminKey').value.trim();
  if (!adminKey) return;
  loadDashboard();
});

document.getElementById('refreshBtn').addEventListener('click', loadDashboard);

async function api(path, options={}) {
  const headers = {...(options.headers || {}), 'X-Admin-Key': adminKey};
  const response = await fetch(`${API}${path}`, {...options, headers});
  if (response.status === 401) throw new Error('Invalid admin key.');
  const data = await response.json();
  if (!response.ok) throw new Error(data.message || 'Request failed.');
  return data;
}

async function loadDashboard() {
  try {
    const [summary, requests, vehicles] = await Promise.all([
      api('/api/dashboard/summary'), api('/api/requests'), fetch(`${API}/api/vehicles`).then(r=>r.json())
    ]);
    loginCard.classList.add('hidden'); dashboard.classList.remove('hidden');
    renderStats(summary); renderRequests(requests); renderVehicles(vehicles);
  } catch (error) {
    document.getElementById('loginError').textContent = error.message;
  }
}

function renderStats(s) {
  const items = [['Total Requests',s.total],['Pending',s.pending],['Confirmed',s.confirmed],['Completed',s.completed]];
  document.getElementById('stats').innerHTML = items.map(x=>`<div class="stat"><div class="num">${x[1]}</div><div class="label">${x[0]}</div></div>`).join('');
}

function renderRequests(requests) {
  const body = document.getElementById('requestsBody');
  if (!requests.length) { body.innerHTML = '<tr><td colspan="6">No requests yet.</td></tr>'; return; }
  body.innerHTML = requests.map(r => `<tr>
    <td><strong>${r.referenceNumber}</strong><br><small>${new Date(r.createdAt).toLocaleDateString()}</small></td>
    <td>${escapeHtml(r.fullName)}<br><small>${escapeHtml(r.phoneNumber)}</small></td>
    <td>${escapeHtml(r.pickupLocation)} → ${escapeHtml(r.destination)}</td>
    <td>${new Date(r.hireDate).toLocaleDateString()}<br>${escapeHtml(r.pickupTime)}</td>
    <td>${escapeHtml(r.vehiclePreference || 'No preference')}<br><small>${r.driverRequired ? 'Driver required' : 'Self-drive'}</small></td>
    <td><select class="status-select" onchange="changeStatus(${r.id},this.value)">${['Pending','Reviewed','Quoted','Confirmed','Completed','Cancelled'].map(s=>`<option ${s===r.status?'selected':''}>${s}</option>`).join('')}</select></td>
  </tr>`).join('');
}

async function changeStatus(id,status){try{await api(`/api/requests/${id}/status`,{method:'PATCH',headers:{'Content-Type':'application/json'},body:JSON.stringify({status})});loadDashboard();}catch(e){alert(e.message);}}

function renderVehicles(vehicles){document.getElementById('vehicles').innerHTML=vehicles.map(v=>`<div class="vehicle-item"><div class="vehicle-title">${escapeHtml(v.name)}</div><div class="vehicle-meta">${escapeHtml(v.type)} • ${escapeHtml(v.registrationNumber)}</div><span class="vehicle-status ${v.status==='Maintenance'?'maintenance':v.status==='On Hire'?'hire':''}">${escapeHtml(v.status)}</span></div>`).join('');}
function escapeHtml(value){return String(value??'').replace(/[&<>'"]/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;',"'":'&#39;','"':'&quot;'}[c]));}
