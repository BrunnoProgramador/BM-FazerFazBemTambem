// ============================================================
// Escolar Manager — utilitários globais
// ============================================================

/**
 * Envia um POST para a URL informada usando o formulário oculto
 * do layout (inclui o token antiforgery automaticamente).
 */
function postar(url) {
    const form = document.getElementById("formPost");
    form.action = url;
    form.submit();
}

/**
 * Pede confirmação e, se aceita, envia o POST.
 * Usada pelos botões de exclusão.
 */
function confirmarPost(url, mensagem) {
    if (confirm(mensagem)) {
        postar(url);
    }
}

/** Escapa HTML para renderização segura em tabelas geradas via JS. */
function escHtml(str) {
    return String(str ?? "")
        .replace(/&/g, "&amp;").replace(/</g, "&lt;")
        .replace(/>/g, "&gt;").replace(/"/g, "&quot;")
        .replace(/'/g, "&#39;");
}

/** Escapa aspas para uso dentro de strings JS geradas dinamicamente. */
function escJs(str) {
    return String(str ?? "").replace(/\\/g, "\\\\").replace(/'/g, "\\'");
}
