// Función para inicializar el checkout de Stripe
window.initStripeCheckout = function (publicKey, sessionId) {
    // Cargar Stripe.js
    if (!window.Stripe) {
        // Si Stripe.js no está cargado, cargar el script
        const script = document.createElement('script');
        script.src = 'https://js.stripe.com/v3/';
        script.onload = function () {
            initStripeRedirect(publicKey, sessionId);
        };
        document.head.appendChild(script);
    } else {
        // Si Stripe.js ya está cargado, inicializar directamente
        initStripeRedirect(publicKey, sessionId);
    }
};

// Función para redirigir al checkout de Stripe
function initStripeRedirect(publicKey, sessionId) {
    try {
        // Inicializar Stripe con la clave pública
        const stripe = Stripe(publicKey);

        // Mostrar mensaje de carga
        const loadingElement = document.getElementById('loading');

        // Redirigir a la página de checkout de Stripe
        stripe.redirectToCheckout({
            sessionId: sessionId
        }).then(function (result) {
            // Si hay un error, mostrarlo
            if (result.error) {
                showError(result.error.message);
            }
        }).catch(function (error) {
            showError(error.message);
        });
    } catch (error) {
        showError(error.message);
    }
}

// Función para mostrar errores
function showError(message) {
    const loadingElement = document.getElementById('loading');
    const errorElement = document.getElementById('error-message');

    if (loadingElement) {
        loadingElement.style.display = 'none';
    }

    if (errorElement) {
        errorElement.textContent = message;
        errorElement.classList.remove('d-none');
    } else {
        alert('Error: ' + message);
    }
}

// Función para cambiar el idioma de la aplicación
window.changeLanguage = function (language) {
    localStorage.setItem('preferredLanguage', language);
    location.reload();
};