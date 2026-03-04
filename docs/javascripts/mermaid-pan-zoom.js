/**
 * mermaid-pan-zoom.js
 *
 * Adds interactive pan/zoom to every rendered Mermaid diagram on Zensical/MkDocs pages,
 * and injects a "View on GitHub" link below each diagram.
 *
 * Dependencies: svg-pan-zoom (loaded via CDN, see below)
 * License: MIT
 */
(function () {
    "use strict";

    var GITHUB_REPO_URL = "https://github.com/openwallet-foundation-labs/sd-jwt-dotnet";
    var SVG_PAN_ZOOM_CDN = "https://cdn.jsdelivr.net/npm/svg-pan-zoom@3.6.2/dist/svg-pan-zoom.min.js";
    var DIAGRAM_MIN_HEIGHT_PX = 300;

    function loadScript(src, callback) {
        var s = document.createElement("script");
        s.src = src;
        s.onload = callback;
        document.head.appendChild(s);
    }

    function enhanceDiagram(svgEl) {
        var container = svgEl.closest(".mermaid");
        if (!container || container.dataset.panZoomEnabled) {
            return;
        }
        container.dataset.panZoomEnabled = "true";

        // Make container relatively sized with visible overflow
        container.style.position = "relative";
        container.style.overflow = "hidden";
        container.style.border = "1px solid var(--md-default-fg-color--lightest, #e0e0e0)";
        container.style.borderRadius = "4px";

        // Ensure the SVG fills its container
        svgEl.setAttribute("width", "100%");
        svgEl.removeAttribute("height");

        var naturalHeight = svgEl.viewBox.baseVal.height || 400;
        var clampedHeight = Math.max(naturalHeight, DIAGRAM_MIN_HEIGHT_PX);
        container.style.height = Math.min(clampedHeight, 600) + "px";

        // Initialise svg-pan-zoom
        var panZoomInstance = svgPanZoom(svgEl, {
            zoomEnabled: true,
            controlIconsEnabled: true,
            fit: true,
            center: true,
            minZoom: 0.5,
            maxZoom: 10,
            zoomScaleSensitivity: 0.3
        });

        // Re-fit when the element becomes visible (e.g. inside tab panels)
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    panZoomInstance.resize();
                    panZoomInstance.fit();
                    panZoomInstance.center();
                }
            });
        });
        observer.observe(container);

        // Add GitHub link below the diagram
        if (!container.nextElementSibling || !container.nextElementSibling.classList.contains("mermaid-github-link")) {
            var linkWrap = document.createElement("p");
            linkWrap.className = "mermaid-github-link";
            linkWrap.style.cssText = "text-align:right;font-size:0.75rem;margin-top:0.25rem;margin-bottom:0.5rem;";

            var a = document.createElement("a");
            a.href = GITHUB_REPO_URL;
            a.textContent = "View source on GitHub";
            a.target = "_blank";
            a.rel = "noopener noreferrer";
            a.style.cssText = "color:var(--md-accent-fg-color,#526cfe);text-decoration:none;";

            linkWrap.appendChild(a);
            container.parentNode.insertBefore(linkWrap, container.nextSibling);
        }
    }

    function enhanceAll() {
        document.querySelectorAll(".mermaid svg").forEach(enhanceDiagram);
    }

    function init() {
        loadScript(SVG_PAN_ZOOM_CDN, function () {
            // Initial pass
            enhanceAll();

            // Mutation observer for diagrams injected after initial render
            var mo = new MutationObserver(function (mutations) {
                var shouldEnhance = mutations.some(function (m) {
                    return Array.from(m.addedNodes).some(function (n) {
                        return n.nodeType === 1 && (n.tagName === "svg" || n.querySelector && n.querySelector("svg"));
                    });
                });
                if (shouldEnhance) {
                    enhanceAll();
                }
            });

            mo.observe(document.body, { childList: true, subtree: true });
        });
    }

    // Zensical/Material uses instant navigation — re-run after each page transition
    if (typeof document$ !== "undefined") {
        // MkDocs Material RxJS observable
        document$.subscribe(init);
    } else {
        document.addEventListener("DOMContentLoaded", init);
    }
})();
