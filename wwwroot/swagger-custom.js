document.addEventListener("DOMContentLoaded", function () {
    if (document.getElementById("swagger-footer")) {
        return;
    }

    var footer = document.createElement("div");
    footer.id = "swagger-footer";
    footer.className = "swagger-footer";
    footer.textContent = "Yopo Backend API";

    var container = document.querySelector(".swagger-ui");
    if (container) {
        container.appendChild(footer);
    } else {
        document.body.appendChild(footer);
    }
});
