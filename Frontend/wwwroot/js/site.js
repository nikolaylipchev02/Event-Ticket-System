document.addEventListener('DOMContentLoaded', function () {
    const authStorageKey = 'event-ticket-system.auth-user';
    const authModalElement = document.getElementById('authModal');
    const authModal = authModalElement ? bootstrap.Modal.getOrCreateInstance(authModalElement) : null;
    const authHeading = document.querySelector('[data-auth-heading]');
    const authFeedback = document.querySelector('[data-auth-feedback]');
    const authTabButtons = Array.from(document.querySelectorAll('[data-auth-tab]'));
    const authForms = Array.from(document.querySelectorAll('[data-auth-form]'));
    const authOpenButtons = Array.from(document.querySelectorAll('[data-auth-open]'));
    const authLogoutButton = document.querySelector('[data-auth-logout]');
    const navbarGuest = document.querySelector('[data-navbar-auth-guest]');
    const navbarUser = document.querySelector('[data-navbar-auth-user]');
    const navbarUserName = document.querySelector('[data-auth-user-name]');
    const navbarUserRole = document.querySelector('[data-auth-user-role]');

    function loadAuthUser() {
        try {
            const rawValue = localStorage.getItem(authStorageKey);
            if (!rawValue) {
                return null;
            }

            const parsedValue = JSON.parse(rawValue);
            if (!parsedValue || typeof parsedValue !== 'object') {
                return null;
            }

            return {
                id: String(parsedValue.id ?? ''),
                name: String(parsedValue.name ?? 'Guest'),
                email: String(parsedValue.email ?? ''),
                role: parsedValue.role ?? 'User'
            };
        } catch {
            return null;
        }
    }

    function persistAuthUser(user) {
        if (user) {
            localStorage.setItem(authStorageKey, JSON.stringify(user));
        } else {
            localStorage.removeItem(authStorageKey);
        }

        authUser = user;
        renderAuthState();
    }

    function normalizeRoleLabel(role) {
        if (typeof role === 'string') {
            return role.toLowerCase() === 'admin' ? 'Admin' : 'User';
        }

        return role === 1 ? 'Admin' : 'User';
    }

    function toAuthUser(data) {
        return {
            id: String(data?.id ?? ''),
            name: String(data?.name ?? 'Guest'),
            email: String(data?.email ?? ''),
            role: normalizeRoleLabel(data?.role)
        };
    }

    function renderAuthState() {
        const signedIn = Boolean(authUser);

        navbarGuest?.classList.toggle('d-none', signedIn);
        navbarUser?.classList.toggle('d-none', !signedIn);

        if (!signedIn) {
            return;
        }

        if (navbarUserName) {
            navbarUserName.textContent = authUser.name;
        }

        if (navbarUserRole) {
            const roleLabel = normalizeRoleLabel(authUser.role);
            navbarUserRole.textContent = roleLabel;
            navbarUserRole.classList.toggle('is-admin', roleLabel === 'Admin');
            navbarUserRole.classList.toggle('is-user', roleLabel !== 'Admin');
        }
    }

    function showFeedback(message, isSuccess) {
        if (!authFeedback) {
            return;
        }

        authFeedback.textContent = message;
        authFeedback.className = `alert mb-4 ${isSuccess ? 'auth-feedback-success' : 'auth-feedback-error'}`;
        authFeedback.classList.remove('d-none');
    }

    function clearFeedback() {
        if (!authFeedback) {
            return;
        }

        authFeedback.className = 'alert d-none mb-4';
        authFeedback.textContent = '';
    }

    function setAuthMode(mode) {
        authForms.forEach(form => {
            form.classList.toggle('d-none', form.getAttribute('data-auth-form') !== mode);
        });

        authTabButtons.forEach(button => {
            button.classList.toggle('is-active', button.getAttribute('data-auth-tab') === mode);
        });

        if (authHeading) {
            authHeading.textContent = mode === 'register' ? 'Create your account' : 'Welcome back';
        }

        clearFeedback();
    }

    function getActiveForm() {
        return authForms.find(form => !form.classList.contains('d-none')) || null;
    }

    function readAuthPayload(form) {
        const formData = new FormData(form);

        if (form.getAttribute('data-auth-form') === 'register') {
            return {
                name: String(formData.get('Name') ?? ''),
                email: String(formData.get('Email') ?? ''),
                password: String(formData.get('Password') ?? ''),
                isAdmin: form.querySelector('[name="IsAdmin"]')?.checked ?? false
            };
        }

        return {
            email: String(formData.get('Email') ?? ''),
            password: String(formData.get('Password') ?? '')
        };
    }

    function normalizeAuthResponseMessage(data, mode) {
        if (data && typeof data === 'object' && data.message) {
            return data.message;
        }

        if (mode === 'register') {
            return `Account created for ${data?.name ?? 'the new user'}.`;
        }

        return `Signed in as ${data?.name ?? 'the user'}.`;
    }

    function openAuthModal(mode) {
        setAuthMode(mode);
        authModal?.show();
    }

    let authUser = loadAuthUser();

    authOpenButtons.forEach(button => {
        button.addEventListener('click', function () {
            openAuthModal(button.getAttribute('data-auth-open') === 'register' ? 'register' : 'login');
        });
    });

    authTabButtons.forEach(button => {
        button.addEventListener('click', function () {
            setAuthMode(button.getAttribute('data-auth-tab') === 'register' ? 'register' : 'login');
        });
    });

    authModalElement?.addEventListener('hidden.bs.modal', function () {
        authForms.forEach(form => form.reset());
        setAuthMode('login');
    });

    authLogoutButton?.addEventListener('click', function () {
        persistAuthUser(null);
    });

    authForms.forEach(form => {
        form.addEventListener('submit', async function (event) {
            event.preventDefault();

            const activeForm = getActiveForm();
            if (!activeForm || activeForm !== form) {
                return;
            }

            const mode = form.getAttribute('data-auth-form') || 'login';
            const submitButton = form.querySelector('button[type="submit"]');
            const previousLabel = submitButton?.textContent ?? '';

            if (submitButton) {
                submitButton.disabled = true;
                submitButton.textContent = mode === 'register' ? 'Creating...' : 'Signing in...';
            }

            try {
                const response = await fetch(`/api/auth/${mode}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(readAuthPayload(form))
                });

                const data = await response.json().catch(() => ({}));

                if (!response.ok) {
                    showFeedback(data.message || 'Something went wrong. Please try again.', false);
                    return;
                }

                persistAuthUser(toAuthUser(data));
                showFeedback(normalizeAuthResponseMessage(data, mode), true);
                form.reset();

                window.setTimeout(() => {
                    authModal?.hide();
                }, 900);
            } catch (error) {
                console.error('Failed to submit auth form:', error);
                showFeedback('The auth popup could not reach the server.', false);
            } finally {
                if (submitButton) {
                    submitButton.disabled = false;
                    submitButton.textContent = previousLabel;
                }
            }
        });
    });

    document.addEventListener('click', async function (event) {
        const authButton = event.target.closest('[data-auth-open]');
        if (authButton) {
            return;
        }

        const button = event.target.closest('[data-copy-event-id]');
        if (!button) {
            return;
        }

        const eventId = button.getAttribute('data-copy-event-id');
        if (!eventId) {
            return;
        }

        try {
            if (navigator.clipboard && window.isSecureContext) {
                await navigator.clipboard.writeText(eventId);
            } else {
                const textarea = document.createElement('textarea');
                textarea.value = eventId;
                textarea.setAttribute('readonly', '');
                textarea.style.position = 'fixed';
                textarea.style.left = '-9999px';
                document.body.appendChild(textarea);
                textarea.select();
                document.execCommand('copy');
                document.body.removeChild(textarea);
            }
        } catch (error) {
            console.error('Failed to copy event id:', error);
        }
    });

    renderAuthState();
});
