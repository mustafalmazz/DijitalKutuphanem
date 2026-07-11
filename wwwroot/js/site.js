// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function showFloatingRewardAnimation(amount) {
    if (!amount || amount <= 0) return;

    var floatingDiv = document.createElement('div');
    floatingDiv.className = 'floating-reward';
    floatingDiv.innerHTML = '<i class="fa-solid fa-gem" style="color: #00bcd4;"></i> +' + amount + ' Bilgelik Taşı';
    document.body.appendChild(floatingDiv);
    
    // Menüdeki (Header) bakiye miktarını görsel olarak güncelle (Sayfayı yenilemeden)
    document.querySelectorAll('.navbar .fa-gem').forEach(function(gemIcon) {
        var textNode = gemIcon.nextSibling;
        if(textNode && textNode.nodeType === 3 && textNode.nodeValue.trim().length > 0) {
            var valStr = textNode.nodeValue.replace('+', '').trim();
            if(!isNaN(parseInt(valStr))) {
                textNode.nodeValue = ' ' + (parseInt(valStr) + amount);
            }
        }
    });

    // Animasyon bitiminde (2.5sn sonra) elementi DOM'dan temizle
    setTimeout(function() {
        if (floatingDiv && floatingDiv.parentNode) {
            floatingDiv.parentNode.removeChild(floatingDiv);
        }
    }, 2500);
}
