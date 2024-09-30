
function formatCurrency(amount) {
    return new Intl.NumberFormat('ja-JP', { style: 'currency', currency: 'JPY' }).format(amount);
}

function loadParameters() {

    try {
        document.getElementById('title').innerText = parameters.title;
        document.getElementById('completion_rate').innerText = "達成率 " + (parameters.filled_amount / parameters.target_amount * 100) + "%";
        document.getElementById('remaining_days').innerText = "残り " + Math.ceil((new Date(parameters.end_date) - new Date())/1000/60/60/24) + "日";
        document.getElementById('project_summary').innerText = parameters.project_summary;
        document.getElementById('project_purpose').innerText = parameters.project_purpose;
        document.getElementById('project_goal').innerText = parameters.project_goal;
        document.getElementById('project_organizer').innerText = parameters.project_organizer;
        document.getElementById('target_amount').innerText = formatCurrency(parameters.target_amount);
        document.getElementById('fund_usage').innerText = parameters.fund_usage;
        document.getElementById('fund_usage_breakdown').innerText = parameters.fund_usage_breakdown;
        document.getElementById('fund_term').innerText = parameters.fund_term;
        document.getElementById('return01_title').innerText = parameters.return01_title;
        document.getElementById('return01_amount').innerText = formatCurrency(parameters.return01_amount);
        document.getElementById('return01_delivery').innerText = parameters.return01_delivery;
        document.getElementById('return01_detail').innerText = parameters.return01_detail;
        document.getElementById('schedule').innerText = parameters.schedule;
    } catch (error) {
        console.error('Error loading parameters:', error);
    }
}

document.addEventListener('DOMContentLoaded', loadParameters);