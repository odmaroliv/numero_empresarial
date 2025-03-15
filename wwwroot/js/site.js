// Funciones comunes utilizadas en toda la aplicación

// Función para mostrar mensajes de alerta temporales
function showAlert(message, type = 'success', duration = 5000) {
    const alertContainer = document.getElementById('alert-container');

    if (!alertContainer) {
        // Crear un contenedor si no existe
        const container = document.createElement('div');
        container.id = 'alert-container';
        container.className = 'position-fixed top-0 end-0 p-3';
        container.style.zIndex = '1050';
        document.body.appendChild(container);
    }

    // Crear el elemento de alerta
    const id = 'alert-' + new Date().getTime();
    const alertHtml = `
        <div id="${id}" class="alert alert-${type} alert-dismissible fade show" role="alert">
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    `;

    // Agregar alerta al contenedor
    const container = document.getElementById('alert-container') || container;
    container.innerHTML += alertHtml;

    // Eliminar automáticamente después de cierto tiempo
    setTimeout(() => {
        const alertElement = document.getElementById(id);
        if (alertElement) {
            const alert = bootstrap.Alert.getOrCreateInstance(alertElement);
            alert.close();
        }
    }, duration);
}

// Formatear valores monetarios
function formatCurrency(amount, currency = 'USD') {
    return new Intl.NumberFormat('es-US', {
        style: 'currency',
        currency: currency
    }).format(amount);
}

// Formatear fechas
function formatDate(dateString, locales = 'es-ES') {
    const date = new Date(dateString);
    return date.toLocaleDateString(locales, {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });
}

// Validación básica de número de teléfono
function isValidPhoneNumber(phone) {
    // Formato básico: +12345678901
    const regex = /^\+[1-9]\d{1,14}$/;
    return regex.test(phone);
}

// Copiar texto al portapapeles
function copyToClipboard(text) {
    navigator.clipboard.writeText(text)
        .then(() => {
            showAlert('Texto copiado al portapapeles', 'success', 2000);
        })
        .catch(err => {
            showAlert('Error al copiar: ' + err, 'danger');
        });
}

// Formatear número de teléfono para visualización
function formatPhoneNumber(phoneNumber) {
    if (!phoneNumber) return '';

    // Si el número ya tiene formato internacional con +, lo dejamos así
    if (phoneNumber.startsWith('+')) {
        return phoneNumber;
    }

    // Si no tiene el +, asumimos que es un número de EE.UU.
    if (phoneNumber.length === 10) {
        return '+1' + phoneNumber;
    }

    return phoneNumber;
}

// Funciones para interactuar con la API de la plataforma
const apiClient = {
    baseUrl: '/api/v1',

    async get(endpoint, params = {}) {
        const url = new URL(this.baseUrl + endpoint, window.location.origin);
        Object.keys(params).forEach(key => url.searchParams.append(key, params[key]));

        const response = await fetch(url, {
            method: 'GET',
            headers: {
                'Accept': 'application/json',
                'Authorization': `Bearer ${this.getToken()}`
            }
        });

        if (!response.ok) {
            throw new Error(`Error API: ${response.status} ${response.statusText}`);
        }

        return response.json();
    },

    async post(endpoint, data = {}) {
        const response = await fetch(this.baseUrl + endpoint, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': `Bearer ${this.getToken()}`
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error(`Error API: ${response.status} ${response.statusText}`);
        }

        return response.json();
    },

    async put(endpoint, data = {}) {
        const response = await fetch(this.baseUrl + endpoint, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json',
                'Authorization': `Bearer ${this.getToken()}`
            },
            body: JSON.stringify(data)
        });

        if (!response.ok) {
            throw new Error(`Error API: ${response.status} ${response.statusText}`);
        }

        return response.json();
    },

    async delete(endpoint) {
        const response = await fetch(this.baseUrl + endpoint, {
            method: 'DELETE',
            headers: {
                'Accept': 'application/json',
                'Authorization': `Bearer ${this.getToken()}`
            }
        });

        if (!response.ok) {
            throw new Error(`Error API: ${response.status} ${response.statusText}`);
        }

        return response.json();
    },

    getToken() {
        return localStorage.getItem('auth_token') || '';
    },

    setToken(token) {
        localStorage.setItem('auth_token', token);
    },

    clearToken() {
        localStorage.removeItem('auth_token');
    }
};

// Funciones específicas para Stripe
async function initStripeCheckout(publicKey, sessionId) {
    if (!publicKey || !sessionId) {
        console.error('Stripe public key or session ID is missing');
        return;
    }

    // Cargar Stripe.js
    if (!window.Stripe) {
        const script = document.createElement('script');
        script.src = 'https://js.stripe.com/v3/';
        script.async = true;
        document.head.appendChild(script);

        // Esperar a que Stripe.js se cargue
        await new Promise(resolve => {
            script.onload = resolve;
        });
    }

    // Inicializar Stripe
    const stripe = Stripe(publicKey);

    try {
        // Redirigir a Checkout
        const { error } = await stripe.redirectToCheckout({
            sessionId: sessionId
        });

        if (error) {
            const errorElement = document.getElementById('error-message');
            if (errorElement) {
                errorElement.textContent = error.message;
                errorElement.classList.remove('d-none');
            }

            console.error('Error con Stripe Checkout:', error);
        }
    } catch (err) {
        console.error('Error iniciando Stripe Checkout:', err);
        const errorElement = document.getElementById('error-message');
        if (errorElement) {
            errorElement.textContent = 'Ocurrió un error al iniciar el proceso de pago. Por favor, inténtelo de nuevo.';
            errorElement.classList.remove('d-none');
        }
    } finally {
        // Ocultar spinner de carga
        const loadingElement = document.getElementById('loading');
        if (loadingElement) {
            loadingElement.classList.add('d-none');
        }
    }
}

// Inicialización de elementos interactivos cuando el DOM está listo
document.addEventListener('DOMContentLoaded', function () {
    // Inicializar tooltips de Bootstrap
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Inicializar popovers de Bootstrap
    const popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Manejar botones de copiar
    const copyButtons = document.querySelectorAll('.btn-copy');
    copyButtons.forEach(button => {
        button.addEventListener('click', function () {
            const textToCopy = this.getAttribute('data-copy');
            if (textToCopy) {
                copyToClipboard(textToCopy);
            }
        });
    });

    // Formatear campos de teléfono automáticamente
    const phoneInputs = document.querySelectorAll('input[type="tel"]');
    phoneInputs.forEach(input => {
        input.addEventListener('blur', function () {
            let value = this.value.trim();
            if (value && !value.startsWith('+')) {
                this.value = '+' + value;
            }
        });
    });
});