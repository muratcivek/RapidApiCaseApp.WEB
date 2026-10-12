
"use strict";

document.addEventListener("DOMContentLoaded", () => {
    initializeCounters();
    initializeChildAgeFields();
    initializeDestinationAutocomplete();
    initializeHotelSearchForm();
});

/* =========================================
   COUNTERS
========================================= */

function initializeCounters() {
    const counters = document.querySelectorAll(".counter");

    counters.forEach(counter => {
        const input = counter.querySelector(".counter-value");

        const minusButton = counter.querySelector(
            '.counter-btn[data-action="minus"]'
        );

        const plusButton = counter.querySelector(
            '.counter-btn[data-action="plus"]'
        );

        if (!input || !minusButton || !plusButton) {
            return;
        }

        const min = Number(counter.dataset.min ?? 0);
        const max = Number(counter.dataset.max ?? 99);

        const updateCounter = value => {
            const nextValue = Math.min(
                max,
                Math.max(min, value)
            );

            input.value = String(nextValue);

            minusButton.disabled = nextValue <= min;
            plusButton.disabled = nextValue >= max;

            counter.dispatchEvent(
                new CustomEvent("counter:change", {
                    bubbles: true,
                    detail: {
                        name: input.name,
                        value: nextValue
                    }
                })
            );
        };

        minusButton.addEventListener("click", () => {
            updateCounter(Number(input.value) - 1);
        });

        plusButton.addEventListener("click", () => {
            updateCounter(Number(input.value) + 1);
        });

        updateCounter(Number(input.value));
    });
}

/* =========================================
   CHILD AGES
========================================= */

function initializeChildAgeFields() {
    const childCounter = document.getElementById("childCounter");
    const childAgeWrapper = document.getElementById("childAgeWrapper");

    if (!childCounter || !childAgeWrapper) {
        return;
    }

    const childInput = childCounter.querySelector(".counter-value");

    if (!childInput) {
        return;
    }

    const existingSelect = childAgeWrapper.querySelector("select");
    const defaultAge = existingSelect?.value ?? "2";

    const container = document.createElement("div");
    container.className = "sf-child-age-list";
    container.id = "childAgeList";

    if (existingSelect) {
        existingSelect.remove();
    }

    childAgeWrapper.appendChild(container);

    const createAgeSelect = (index, selectedAge) => {
        const select = document.createElement("select");

        select.name = "childAges";
        select.id = `childAge_${index}`;
        select.className = "sf-age-select";

        select.setAttribute(
            "aria-label",
            `${index + 1}. çocuk yaþý`
        );

        for (let age = 0; age <= 17; age++) {
            const option = document.createElement("option");

            option.value = String(age);
            option.textContent = `${age} yaþ`;

            if (String(age) === String(selectedAge)) {
                option.selected = true;
            }

            select.appendChild(option);
        }

        return select;
    };

    const renderChildAges = () => {
        const count = Number(childInput.value);

        const selectedAges = Array.from(
            container.querySelectorAll("select")
        ).map(select => select.value);

        childAgeWrapper.hidden = count === 0;

        if (count === 0) {
            container.replaceChildren();
            return;
        }

        const fragment = document.createDocumentFragment();

        for (let i = 0; i < count; i++) {
            const age = selectedAges[i] ?? defaultAge;

            fragment.appendChild(
                createAgeSelect(i, age)
            );
        }

        container.replaceChildren(fragment);
    };

    childCounter.addEventListener(
        "counter:change",
        renderChildAges
    );

    renderChildAges();
}

/* =========================================
   DESTINATION AUTOCOMPLETE
========================================= */

function initializeDestinationAutocomplete() {
    const form = document.getElementById("hotelSearchForm");
    const input = document.getElementById("destination");
    const destIdInput = document.getElementById("destId");
    const searchTypeInput = document.getElementById("searchType");
    const results = document.getElementById("destinationResults");
    const message = document.getElementById("destinationMessage");
    const field = document.getElementById("destinationField");

    if (
        !form ||
        !input ||
        !destIdInput ||
        !searchTypeInput ||
        !results ||
        !field
    ) {
        return;
    }

    const endpoint = form.dataset.destinationUrl;

    if (!endpoint) {
        return;
    }

    let debounceTimer = null;
    let abortController = null;
    let activeIndex = -1;
    let currentItems = [];

    const setMessage = text => {
        if (message) {
            message.textContent = text;
        }
    };

    const closeResults = () => {
        results.hidden = true;

        input.setAttribute(
            "aria-expanded",
            "false"
        );

        input.removeAttribute(
            "aria-activedescendant"
        );

        activeIndex = -1;
    };

    const openResults = () => {
        if (currentItems.length === 0) {
            return;
        }

        results.hidden = false;

        input.setAttribute(
            "aria-expanded",
            "true"
        );
    };

    const clearSelection = () => {
        destIdInput.value = "";
        searchTypeInput.value = "";
    };

    const selectDestination = item => {
        input.value = item.name;
        destIdInput.value = item.destId;
        searchTypeInput.value = item.searchType;

        clearTimeout(debounceTimer);
        abortController?.abort();

        currentItems = [];
        results.replaceChildren();

        closeResults();
        setMessage("");
    };

    const updateActiveOption = () => {
        const options = results.querySelectorAll(
            '[role="option"]'
        );

        options.forEach((option, index) => {
            const isActive = index === activeIndex;

            option.classList.toggle(
                "is-active",
                isActive
            );

            option.setAttribute(
                "aria-selected",
                String(isActive)
            );
        });

        if (
            activeIndex >= 0 &&
            options[activeIndex]
        ) {
            const activeOption = options[activeIndex];

            input.setAttribute(
                "aria-activedescendant",
                activeOption.id
            );

            activeOption.scrollIntoView({
                block: "nearest"
            });
        } else {
            input.removeAttribute(
                "aria-activedescendant"
            );
        }
    };

    const createDestinationItem = (item, index) => {
        const option = document.createElement("div");

        option.className = "sf-destination-option";
        option.id = `destination-option-${index}`;

        option.setAttribute("role", "option");
        option.setAttribute("aria-selected", "false");

        const media = document.createElement("div");
        media.className = "sf-destination-option-media";

        if (item.imageUrl) {
            const image = document.createElement("img");

            image.src = item.imageUrl;
            image.alt = "";
            image.loading = "lazy";

            media.appendChild(image);
        } else {
            const icon = document.createElement("i");

            icon.className = "bi bi-geo-alt";

            media.appendChild(icon);
        }

        const content = document.createElement("div");
        content.className = "sf-destination-option-content";

        const title = document.createElement("strong");
        title.textContent = item.name;

        const subtitle = document.createElement("span");

        subtitle.textContent =
            item.label ||
            item.country ||
            "";

        content.appendChild(title);
        content.appendChild(subtitle);

        const meta = document.createElement("div");
        meta.className = "sf-destination-option-meta";

        if (item.hotelCount != null) {
            const count = document.createElement("span");

            count.textContent =
                `${Number(item.hotelCount)
                    .toLocaleString("tr-TR")} otel`;

            meta.appendChild(count);
        }

        option.appendChild(media);
        option.appendChild(content);
        option.appendChild(meta);

        // Mouse seçimi sýrasýnda input blur olsa bile
        // destinasyon seçiminin gerçekleþmesini saðlar.
        option.addEventListener("mousedown", event => {
            event.preventDefault();
            selectDestination(item);
        });

        // Klavye veya dokunmatik etkileþim için.
        option.addEventListener("click", () => {
            selectDestination(item);
        });

        return option;
    };

    const renderResults = items => {
        currentItems = items;
        activeIndex = -1;

        results.replaceChildren();

        if (items.length === 0) {
            closeResults();

            setMessage(
                "Eþleþen destinasyon bulunamadý."
            );

            return;
        }

        const fragment = document.createDocumentFragment();

        items.forEach((item, index) => {
            fragment.appendChild(
                createDestinationItem(item, index)
            );
        });

        results.appendChild(fragment);

        setMessage("");
        openResults();
    };

    const searchDestinations = async query => {
        abortController?.abort();

        const controller = new AbortController();
        abortController = controller;

        setMessage("Destinasyonlar aranýyor...");

        try {
            const url = new URL(
                endpoint,
                window.location.origin
            );

            url.searchParams.set("query", query);

            const response = await fetch(url, {
                method: "GET",
                signal: controller.signal,
                headers: {
                    "Accept": "application/json"
                }
            });

            if (!response.ok) {
                throw new Error(
                    `HTTP ${response.status}`
                );
            }

            const data = await response.json();

            if (
                controller.signal.aborted ||
                input.value.trim() !== query
            ) {
                return;
            }

            renderResults(
                Array.isArray(data) ? data : []
            );
        } catch (error) {
            if (error.name === "AbortError") {
                return;
            }

            console.error(
                "Destinasyon arama hatasý:",
                error
            );

            closeResults();

            setMessage(
                "Destinasyonlar yüklenemedi."
            );
        }
    };

    input.addEventListener("input", () => {
        clearSelection();
        closeResults();

        results.replaceChildren();
        currentItems = [];

        clearTimeout(debounceTimer);
        abortController?.abort();

        const query = input.value.trim();

        if (query.length < 2) {
            setMessage("");
            return;
        }

        debounceTimer = setTimeout(() => {
            searchDestinations(query);
        }, 350);
    });

    input.addEventListener("keydown", event => {
        if (
            results.hidden ||
            currentItems.length === 0
        ) {
            return;
        }

        if (event.key === "ArrowDown") {
            event.preventDefault();

            activeIndex = Math.min(
                activeIndex + 1,
                currentItems.length - 1
            );

            updateActiveOption();
        }

        if (event.key === "ArrowUp") {
            event.preventDefault();

            activeIndex = activeIndex <= 0
                ? currentItems.length - 1
                : activeIndex - 1;

            updateActiveOption();
        }

        if (
            event.key === "Enter" &&
            activeIndex >= 0
        ) {
            event.preventDefault();

            selectDestination(
                currentItems[activeIndex]
            );
        }

        if (event.key === "Escape") {
            closeResults();
        }
    });

    document.addEventListener("mousedown", event => {
        if (!field.contains(event.target)) {
            closeResults();
        }
    });

    input.addEventListener("focus", () => {
        if (
            currentItems.length > 0 &&
            !destIdInput.value
        ) {
            openResults();
        }
    });

}


/* =========================================
   HOTEL SEARCH - BACKEND VALIDATION
========================================= */

function initializeHotelSearchForm() {
    const form = document.getElementById("hotelSearchForm");
    const errorBox = document.getElementById("hotelSearchError");

    if (!form || !errorBox) return;

    const validationUrl = form.dataset.validationUrl;
    const submitButton = form.querySelector(
        'button[type="submit"]'
    );

    if (!validationUrl) return;

    let isSubmitting = false;

    const clearErrors = () => {
        errorBox.replaceChildren();
        errorBox.hidden = true;

        form.querySelectorAll(".sf-field-invalid")
            .forEach(element => {
                element.classList.remove("sf-field-invalid");
            });
    };

    const showErrors = errors => {
        clearErrors();

        const list = document.createElement("ul");
        list.className = "sf-validation-list";

        const uniqueMessages = new Set();

        errors.forEach(error => {
            if (!error?.message) return;

            if (!uniqueMessages.has(error.message)) {
                uniqueMessages.add(error.message);

                const item = document.createElement("li");
                item.textContent = error.message;
                list.appendChild(item);
            }

            // Backend'den gelen alan adýna göre
            // ilgili form alanýný iþaretle.
            let fieldName = error.field;

            if (
                fieldName === "DestId" ||
                fieldName === "SearchType"
            ) {
                fieldName = "Destination";
            }

            const input = Array.from(form.elements)
                .find(element =>
                    element.name?.toLowerCase() ===
                    fieldName?.toLowerCase()
                );

            if (input) {
                const wrapper = input.closest(
                    ".sf-search-field, .sf-counter-field"
                );

                wrapper?.classList.add("sf-field-invalid");
            }
        });

        if (!list.children.length) {
            const item = document.createElement("li");
            item.textContent = "Arama bilgileri geçersiz.";
            list.appendChild(item);
        }

        errorBox.appendChild(list);
        errorBox.hidden = false;
    };

    const setLoading = loading => {
        if (!submitButton) return;

        submitButton.disabled = loading;
        submitButton.classList.toggle(
            "is-loading",
            loading
        );
    };

    form.addEventListener("submit", async event => {
        event.preventDefault();

        if (isSubmitting) return;

        clearErrors();
        isSubmitting = true;
        setLoading(true);

        const formData = new FormData(form);
        const parameters = new URLSearchParams(formData);

        try {
            const url = new URL(
                validationUrl,
                window.location.origin
            );

            url.search = parameters.toString();

            const response = await fetch(url, {
                method: "GET",
                headers: {
                    "Accept": "application/json"
                }
            });

            const result = await response.json();

            if (!response.ok || !result.valid) {
                showErrors(result.errors ?? []);
                return;
            }

            // Backend doðrulamasý baþarýlýysa
            // otel sonuç sayfasýna geç.
            const searchUrl = new URL(
                form.action,
                window.location.origin
            );

            searchUrl.search = parameters.toString();

            window.location.assign(searchUrl.toString());

        } catch (error) {
            console.error(
                "Backend doðrulama hatasý:",
                error
            );

            showErrors([
                {
                    field: "",
                    message: "Sunucuya ulaþýlamadý. Lütfen tekrar deneyin."
                }
            ]);
        } finally {
            isSubmitting = false;
            setLoading(false);
        }
    });

    // Kullanýcý alanlarý düzenlediðinde
    // önceki doðrulama hatalarýný temizle.
    form.addEventListener("input", clearErrors);
    form.addEventListener("change", clearErrors);
}
