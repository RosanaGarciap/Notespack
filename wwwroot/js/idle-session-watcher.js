// idle-session-watcher.js
//
// Flujo: al detectar inactividad se MUESTRA UN AVISO (no se cierra sesión
// todavía). El aviso dura 5 segundos. Si el usuario responde a tiempo
// ("Continuar Trabajando"), se cancela todo y la sesión sigue viva. Si no
// responde, o pulsa "Salir", ahí sí se invalida la cookie y se redirige.

const NotespackIdle = {
    Var_TimerId: null,
    Var_TimeoutMs: 2 * 60 * 1000, // se sobreescribe al iniciar
    Var_LogoutEndpoint: "/Account/InactivityLogout",
    Var_LoginRedirectUrl: "/login?motivo=inactividad",
    Var_ActivityEvents: ["mousemove", "mousedown", "keydown", "scroll", "touchstart", "click"],
    Var_ModalAutoCloseMs: 5000, // duración fija del aviso: 5 segundos
    Var_ModalTimerId: null,
    Var_IsModalOpen: false,

    Var_ResetTimer() {
        // Mientras el modal de aviso está abierto, la actividad normal del
        // mouse/teclado no debe reiniciar nada por sí sola: solo el botón
        // "Continuar Trabajando" puede cancelar el cierre. Esto evita que
        // un movimiento accidental del mouse tape el aviso sin que el
        // usuario lo haya visto.
        if (this.Var_IsModalOpen) return;

        if (this.Var_TimerId) clearTimeout(this.Var_TimerId);
        this.Var_TimerId = setTimeout(() => this.Var_ShowExpiredSessionModal(), this.Var_TimeoutMs);
    },

    // --- Construcción del modal --------------------------------------

    Var_ShowExpiredSessionModal() {
        this.Var_IsModalOpen = true;
        this.Var_InjectModalStyles();

        const overlay = document.createElement("div");
        overlay.id = "notespack-idle-overlay";
        overlay.innerHTML = `
            <div class="notespack-idle-card" role="alertdialog" aria-modal="true" aria-labelledby="notespack-idle-title">
                <div class="notespack-idle-icon" aria-hidden="true">
                    <svg viewBox="0 0 24 24" width="56" height="56" fill="none" stroke="currentColor" stroke-width="2">
                        <circle cx="12" cy="12" r="10"></circle>
                        <line x1="12" y1="7" x2="12" y2="13"></line>
                        <circle cx="12" cy="16.5" r="0.9" fill="currentColor" stroke="none"></circle>
                    </svg>
                </div>
                <h2 id="notespack-idle-title" class="notespack-idle-title">Tu sesión ha expirado</h2>
                <div class="notespack-idle-actions">
                    <button type="button" class="notespack-idle-btn notespack-idle-btn-secondary" id="notespack-idle-continue">
                        Continuar Trabajando
                    </button>
                    <button type="button" class="notespack-idle-btn notespack-idle-btn-danger" id="notespack-idle-exit">
                        Salir
                    </button>
                </div>
            </div>
        `;
        document.body.appendChild(overlay);

        document.getElementById("notespack-idle-exit")
            .addEventListener("click", () => this.Var_ConfirmLogoutNow());

        document.getElementById("notespack-idle-continue")
            .addEventListener("click", () => this.Var_CancelLogoutAndResume());

        // Si nadie responde en 5 segundos, se procede a cerrar sesión de verdad.
        this.Var_ModalTimerId = setTimeout(() => this.Var_ConfirmLogoutNow(), this.Var_ModalAutoCloseMs);
    },

    Var_RemoveModal() {
        if (this.Var_ModalTimerId) {
            clearTimeout(this.Var_ModalTimerId);
            this.Var_ModalTimerId = null;
        }
        const overlay = document.getElementById("notespack-idle-overlay");
        if (overlay) overlay.remove();
        this.Var_IsModalOpen = false;
    },

    // El usuario NO respondió a tiempo, o pulsó "Salir": recién aquí se
    // invalida la cookie en el servidor y se redirige.
    async Var_ConfirmLogoutNow() {
        this.Var_RemoveModal();
        try {
            await fetch(this.Var_LogoutEndpoint, { method: "POST" });
        } finally {
            window.location.href = this.Var_LoginRedirectUrl;
        }
    },

    // El usuario pulsó "Continuar Trabajando" a tiempo: la cookie nunca se
    // tocó, así que no hay nada que restaurar. Solo se cierra el aviso y se
    // reinicia el conteo de inactividad desde cero.
    Var_CancelLogoutAndResume() {
        this.Var_RemoveModal();
        this.Var_ResetTimer();
    },

    Var_InjectModalStyles() {
        if (document.getElementById("notespack-idle-styles")) return;

        const style = document.createElement("style");
        style.id = "notespack-idle-styles";
        style.textContent = `
            #notespack-idle-overlay {
                position: fixed;
                inset: 0;
                background: rgba(30, 30, 30, 0.55);
                display: flex;
                align-items: center;
                justify-content: center;
                z-index: 2147483647;
            }
            .notespack-idle-card {
                background: #ffffff;
                border-radius: 12px;
                padding: 2.5rem 2rem;
                max-width: 420px;
                width: 90%;
                text-align: center;
                box-shadow: 0 20px 50px rgba(0,0,0,0.35);
                font-family: system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            }
            .notespack-idle-icon {
                color: #F2B155;
                display: flex;
                justify-content: center;
                margin-bottom: 1.25rem;
            }
            .notespack-idle-title {
                color: #222;
                font-size: 1.6rem;
                font-weight: 500;
                margin: 0 0 1.75rem 0;
            }
            .notespack-idle-actions {
                display: flex;
                gap: 0.75rem;
                justify-content: center;
                flex-wrap: wrap;
            }
            .notespack-idle-btn {
                border: none;
                border-radius: 6px;
                padding: 0.75rem 1.5rem;
                font-size: 1rem;
                cursor: pointer;
            }
            .notespack-idle-btn-secondary {
                background: #f1f1f1;
                color: #333;
            }
            .notespack-idle-btn-secondary:hover {
                background: #e4e4e4;
            }
            .notespack-idle-btn-danger {
                background: #b83b3b;
                color: #fff;
                box-shadow: 0 0 12px rgba(184, 59, 59, 0.5);
            }
            .notespack-idle-btn-danger:hover {
                background: #a53333;
            }
        `;
        document.head.appendChild(style);
    },

    // --- Arranque / detención del vigilante ---------------------------

    Var_Start(minutosLimite) {
        this.Var_TimeoutMs = minutosLimite * 60 * 1000;
        this.Var_IsModalOpen = false;

        this.Var_ActivityEvents.forEach((nombreEvento) => {
            document.addEventListener(nombreEvento, this.Var_BoundReset, { passive: true });
        });

        this.Var_ResetTimer();
    },

    Var_Stop() {
        if (this.Var_TimerId) {
            clearTimeout(this.Var_TimerId);
            this.Var_TimerId = null;
        }
        this.Var_RemoveModal();

        this.Var_ActivityEvents.forEach((nombreEvento) => {
            document.removeEventListener(nombreEvento, this.Var_BoundReset);
        });
    }
};

NotespackIdle.Var_BoundReset = NotespackIdle.Var_ResetTimer.bind(NotespackIdle);

export function Var_StartIdleWatcher(minutosLimite) {
    NotespackIdle.Var_Start(minutosLimite);
}

export function Var_StopIdleWatcher() {
    NotespackIdle.Var_Stop();
}