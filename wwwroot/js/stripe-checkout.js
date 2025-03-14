// Funci�n para inicializar el checkout de Stripe
window.initStripeCheckout = function (publicKey, sessionId) {
    // Cargar Stripe.js
    if (!window.Stripe) {
        // Si Stripe.js no est� cargado, cargar el script
        const script = document.createElement('script');
        script.src = 'https://js.stripe.com/v3/';
        script.onload = function () {
            initStripeRedirect(publicKey, sessionId);
        };
        document.head.appendChild(script);
    } else {
        // Si Stripe.js ya est� cargado, inicializar directamente
        initStripeRedirect(publicKey, sessionId);
    }
};

// Funci�n para redirigir al checkout de Stripe
function initStripeRedirect(publicKey, sessionId) {
    try {
        // Inicializar Stripe con la clave p�blica
        const stripe = Stripe(publicKey);

        // Mostrar mensaje de carga
        const loadingElement = document.getElementById('loading');

        // Redirigir a la p�gina de checkout de Stripe
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

// Funci�n para mostrar errores
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

// Funci�n para cambiar el idioma de la aplicaci�n
window.changeLanguage = function (language) {
    localStorage.setItem('preferredLanguage', language);
    location.reload();
};