// Password validation function
function getPasswordValidationMessages(password, confirmPassword) {
	const isEditPasswordEnabledField = document.getElementById('isEditPasswordEnabled');
	if (isEditPasswordEnabledField.value === 'false') {
		return [];
	}

	let messages = [];

	if (password.length < 8) messages.push("Must be at least 8 characters");
	if (!/[A-Z]/.test(password)) messages.push("Must include an uppercase letter");
	if (!/[a-z]/.test(password)) messages.push("Must include a lowercase letter");
	if (!/[0-9]/.test(password)) messages.push("Must include a number");
	if (!/[!@#$%^&*(),.?\":{}|<>]/.test(password)) messages.push("Must include a special character");
	if (confirmPassword && password !== confirmPassword) messages.push("Passwords do not match");

	return messages;
}

// Validate password and update messages & progress bar
function validatePassword() {
	const passwordField = document.getElementById('passwordField');
	const confirmPasswordField = document.getElementById('confirmPassword');
	const isEditPasswordEnabledField = document.getElementById('isEditPasswordEnabled');
	const messageBox = document.getElementById("confirmPasswordMessage");
	const strengthBar = document.getElementById("passwordStrengthBar");
	const strengthText = document.getElementById("passwordStrengthText");

	if (isEditPasswordEnabledField.value === 'false') {
		// Reset everything when editing is disabled
		strengthBar.style.width = "0%";
		strengthBar.className = "progress-bar";
		strengthText.textContent = "";
		messageBox.textContent = "";
		return;
	}

	const password = passwordField.value;
	const confirmPassword = confirmPasswordField.value;
	const messages = getPasswordValidationMessages(password, confirmPassword);

	if (messages.length > 0) {
		messageBox.textContent = messages.join(" | ");
		messageBox.style.color = "red";
	} else {
		if (confirmPassword) {
			messageBox.textContent = "Passwords match ✅";
		} else {
			messageBox.textContent = "Password looks good ✅";
		}
		messageBox.style.color = "green";
	}

	updatePasswordStrength(password);
}

// Update password strength bar
function updatePasswordStrength(password) {
	const isEditPasswordEnabledField = document.getElementById('isEditPasswordEnabled');
	if (isEditPasswordEnabledField.value === 'false') return;

	let strength = 0;
	if (password.length >= 8) strength++;
	if (/[A-Z]/.test(password)) strength++;
	if (/[a-z]/.test(password)) strength++;
	if (/[0-9]/.test(password)) strength++;
	if (/[!@#$%^&*(),.?":{}|<>]/.test(password)) strength++;

	const strengthBar = document.getElementById("passwordStrengthBar");
	const strengthText = document.getElementById("passwordStrengthText");

	let strengthPercent = (strength / 5) * 100;
	strengthBar.style.width = strengthPercent + "%";

	if (strength <= 2) {
		strengthBar.className = "progress-bar bg-danger";
		strengthText.textContent = "Weak";
	} else if (strength === 3 || strength === 4) {
		strengthBar.className = "progress-bar bg-warning";
		strengthText.textContent = "Medium";
	} else if (strength === 5) {
		strengthBar.className = "progress-bar bg-success";
		strengthText.textContent = "Strong";
	}
}

// --- Live validation ---
document.getElementById('passwordField').addEventListener('input', validatePassword);
document.getElementById('confirmPassword').addEventListener('input', validatePassword);

// --- Also hook toggle button to update messages/progress ---
document.getElementById('togglePasswordButton').addEventListener('click', function () {
	setTimeout(validatePassword, 50); // small delay to let the button update the field state
});

// --- Prevent form submission if validation fails ---
document.querySelector('form').addEventListener('submit', function (e) {
	const passwordField = document.getElementById('passwordField');
	const confirmPasswordField = document.getElementById('confirmPassword');
	const isEditPasswordEnabledField = document.getElementById('isEditPasswordEnabled');
	const messages = getPasswordValidationMessages(passwordField.value, confirmPasswordField.value);

	if (isEditPasswordEnabledField.value === 'true' && messages.length > 0) {
		e.preventDefault();
		const messageBox = document.getElementById("confirmPasswordMessage");
		messageBox.textContent = messages.join(" | ");
		messageBox.style.color = "red";
	}
});
