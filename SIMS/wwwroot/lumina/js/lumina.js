(function () {
  'use strict';

  var themeKey = 'sims-theme';
  var fontScaleKey = 'sims-font-scale';
  var contrastKey = 'sims-contrast';
  var viewKey = 'sims-course-view';

  function storageGet(key) {
    try {
      return window.localStorage.getItem(key);
    } catch (error) {
      return null;
    }
  }

  function storageSet(key, value) {
    try {
      window.localStorage.setItem(key, value);
    } catch (error) {
      return;
    }
  }

  function applyTheme(isDark) {
    document.documentElement.classList.toggle('dark', isDark);
    document.querySelectorAll('[data-theme-icon]').forEach(function (icon) {
      icon.textContent = isDark ? 'dark_mode' : 'light_mode';
    });
    document.querySelectorAll('[data-theme-toggle]').forEach(function (button) {
      button.setAttribute('aria-pressed', isDark ? 'true' : 'false');
      button.setAttribute('title', isDark ? 'Switch to light mode' : 'Switch to dark mode');
    });
  }

  function initTheme() {
    var stored = storageGet(themeKey);
    var prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    var isDark = stored === 'dark' || (stored !== 'light' && prefersDark);
    applyTheme(isDark);

    document.querySelectorAll('[data-theme-toggle]').forEach(function (button) {
      button.addEventListener('click', function () {
        var next = !document.documentElement.classList.contains('dark');
        applyTheme(next);
        storageSet(themeKey, next ? 'dark' : 'light');
      });
    });
  }

  function initSidebar() {
    var sidebar = document.getElementById('lumina-sidebar') || document.querySelector('[data-sidebar]');
    var backdrop = document.querySelector('[data-sidebar-backdrop]');
    if (!sidebar) {
      return;
    }

    function openSidebar() {
      sidebar.classList.remove('-translate-x-full');
      sidebar.classList.add('translate-x-0');
      if (backdrop) {
        backdrop.classList.remove('hidden');
      }
      document.body.classList.add('overflow-hidden', 'lg:overflow-auto');
    }

    function closeSidebar() {
      sidebar.classList.add('-translate-x-full');
      sidebar.classList.remove('translate-x-0');
      if (backdrop) {
        backdrop.classList.add('hidden');
      }
      document.body.classList.remove('overflow-hidden', 'lg:overflow-auto');
    }

    document.querySelectorAll('[data-sidebar-toggle]').forEach(function (button) {
      button.addEventListener('click', function () {
        if (sidebar.classList.contains('-translate-x-full')) {
          openSidebar();
        } else {
          closeSidebar();
        }
      });
    });

    if (backdrop) {
      backdrop.addEventListener('click', closeSidebar);
    }

    sidebar.querySelectorAll('a').forEach(function (link) {
      link.addEventListener('click', function () {
        if (window.innerWidth < 1024) {
          closeSidebar();
        }
      });
    });

    document.addEventListener('keydown', function (event) {
      if (event.key === 'Escape') {
        closeSidebar();
      }
    });
  }

  function closeDropdowns(except) {
    document.querySelectorAll('[data-dropdown]').forEach(function (dropdown) {
      if (dropdown === except) {
        return;
      }
      var panel = dropdown.querySelector('[data-dropdown-panel]');
      var toggle = dropdown.querySelector('[data-dropdown-toggle]');
      if (panel) {
        panel.classList.add('hidden');
      }
      if (toggle) {
        toggle.setAttribute('aria-expanded', 'false');
      }
    });
  }

  function initDropdowns() {
    document.querySelectorAll('[data-dropdown]').forEach(function (dropdown) {
      var toggle = dropdown.querySelector('[data-dropdown-toggle]');
      var panel = dropdown.querySelector('[data-dropdown-panel]');
      if (!toggle || !panel) {
        return;
      }

      panel.classList.add('hidden');
      toggle.setAttribute('aria-expanded', 'false');

      toggle.addEventListener('click', function (event) {
        event.preventDefault();
        event.stopPropagation();
        var willOpen = panel.classList.contains('hidden');
        closeDropdowns(dropdown);
        panel.classList.toggle('hidden', !willOpen);
        toggle.setAttribute('aria-expanded', willOpen ? 'true' : 'false');
      });

      panel.addEventListener('click', function (event) {
        event.stopPropagation();
      });
    });

    document.addEventListener('click', function () {
      closeDropdowns(null);
    });

    document.addEventListener('keydown', function (event) {
      if (event.key === 'Escape') {
        closeDropdowns(null);
      }
    });
  }

  function initTabs() {
    document.querySelectorAll('[data-tabs]').forEach(function (group) {
      var buttons = Array.prototype.slice.call(group.querySelectorAll('[data-tab-target]'));
      if (buttons.length === 0) {
        return;
      }

      function activate(target, button) {
        buttons.forEach(function (item) {
          item.classList.remove('tab-active');
          item.setAttribute('aria-selected', 'false');
        });
        button.classList.add('tab-active');
        button.setAttribute('aria-selected', 'true');

        document.querySelectorAll('[data-tab-panel]').forEach(function (panel) {
          panel.classList.toggle('hidden', panel.getAttribute('data-tab-panel') !== target);
        });
      }

      buttons.forEach(function (button) {
        button.addEventListener('click', function () {
          activate(button.getAttribute('data-tab-target'), button);
        });
      });

      var initial = group.querySelector('[data-tab-target].tab-active') || buttons[0];
      activate(initial.getAttribute('data-tab-target'), initial);
    });
  }

  function openModal(modal) {
    if (!modal) {
      return;
    }
    modal.classList.remove('hidden');
    document.body.classList.add('overflow-hidden');
    var focusable = modal.querySelector('input:not([type="hidden"]), select, textarea, button, [href]');
    if (focusable) {
      window.setTimeout(function () {
        focusable.focus();
      }, 40);
    }
  }

  function closeModal(modal) {
    if (!modal) {
      return;
    }
    modal.classList.add('hidden');
    if (!document.querySelector('.modal-backdrop:not(.hidden)')) {
      document.body.classList.remove('overflow-hidden');
    }
  }

  function initModals() {
    document.querySelectorAll('[data-modal-open]').forEach(function (button) {
      button.addEventListener('click', function (event) {
        event.preventDefault();
        openModal(document.getElementById(button.getAttribute('data-modal-open')));
      });
    });

    document.querySelectorAll('[data-modal-close]').forEach(function (button) {
      button.addEventListener('click', function () {
        closeModal(button.closest('.modal-backdrop') || button.closest('[data-modal]'));
      });
    });

    document.querySelectorAll('.modal-backdrop').forEach(function (backdrop) {
      backdrop.addEventListener('click', function (event) {
        if (event.target === backdrop) {
          closeModal(backdrop);
        }
      });
    });

    document.addEventListener('keydown', function (event) {
      if (event.key === 'Escape') {
        document.querySelectorAll('.modal-backdrop:not(.hidden)').forEach(function (modal) {
          closeModal(modal);
        });
      }
    });
  }

  function initPasswordToggles() {
    document.querySelectorAll('[data-password-toggle]').forEach(function (button) {
      button.addEventListener('click', function () {
        var targetId = button.getAttribute('data-password-target');
        var wrapper = button.closest('.relative') || button.parentElement;
        var input = targetId ? document.getElementById(targetId) : wrapper.querySelector('input');
        if (!input) {
          return;
        }

        var isPassword = input.getAttribute('type') === 'password';
        input.setAttribute('type', isPassword ? 'text' : 'password');
        var icon = button.querySelector('[data-password-icon]') || button.querySelector('.material-symbols-outlined');
        if (icon) {
          icon.textContent = isPassword ? 'visibility_off' : 'visibility';
        }
        button.setAttribute('aria-label', isPassword ? 'Hide password' : 'Show password');
      });
    });
  }

  function initConfirmForms() {
    document.querySelectorAll('form[data-confirm]').forEach(function (form) {
      form.addEventListener('submit', function (event) {
        if (form.getAttribute('data-confirmed') === 'true') {
          return;
        }
        var message = form.getAttribute('data-confirm') || 'Are you sure you want to continue?';
        if (!window.confirm(message)) {
          event.preventDefault();
          return;
        }
        form.setAttribute('data-confirmed', 'true');
      });
    });
  }

  function initSelectAll() {
    var selectAll = document.querySelector('[data-select-all]');
    var rowCheckboxes = Array.prototype.slice.call(document.querySelectorAll('[data-row-checkbox]'));
    var bulkBar = document.querySelector('[data-bulk-bar]');
    if (!selectAll && rowCheckboxes.length === 0) {
      return;
    }

    function syncBulkBar() {
      if (!bulkBar) {
        return;
      }
      var anyChecked = rowCheckboxes.some(function (checkbox) {
        return checkbox.checked;
      });
      bulkBar.classList.toggle('hidden', !anyChecked);
    }

    if (selectAll) {
      selectAll.addEventListener('change', function () {
        rowCheckboxes.forEach(function (checkbox) {
          checkbox.checked = selectAll.checked;
        });
        syncBulkBar();
      });
    }

    rowCheckboxes.forEach(function (checkbox) {
      checkbox.addEventListener('change', function () {
        if (selectAll) {
          selectAll.checked = rowCheckboxes.length > 0 && rowCheckboxes.every(function (item) {
            return item.checked;
          });
          selectAll.indeterminate = rowCheckboxes.some(function (item) {
            return item.checked;
          }) && !selectAll.checked;
        }
        syncBulkBar();
      });
    });

    syncBulkBar();
  }

  function applyView(view) {
    document.querySelectorAll('[data-view-panel]').forEach(function (panel) {
      panel.classList.toggle('hidden', panel.getAttribute('data-view-panel') !== view);
    });
    document.querySelectorAll('[data-view-toggle]').forEach(function (button) {
      var active = button.getAttribute('data-view-toggle') === view;
      button.classList.toggle('tab-active', active);
      button.setAttribute('aria-pressed', active ? 'true' : 'false');
    });
  }

  function initViewToggle() {
    var buttons = document.querySelectorAll('[data-view-toggle]');
    if (buttons.length === 0) {
      return;
    }

    buttons.forEach(function (button) {
      button.addEventListener('click', function () {
        var view = button.getAttribute('data-view-toggle');
        applyView(view);
        storageSet(viewKey, view);
      });
    });

    var stored = storageGet(viewKey);
    var fallback = buttons[0].getAttribute('data-view-toggle');
    applyView(stored === 'grid' || stored === 'table' ? stored : fallback);
  }

  function dismissToast(toast) {
    if (!toast) {
      return;
    }
    toast.style.transition = 'opacity 200ms ease, transform 200ms ease';
    toast.style.opacity = '0';
    toast.style.transform = 'translateX(0.75rem)';
    window.setTimeout(function () {
      toast.remove();
    }, 220);
  }

  function initToasts() {
    document.querySelectorAll('[data-toast]').forEach(function (toast) {
      window.setTimeout(function () {
        dismissToast(toast);
      }, 5000);

      var closeButton = toast.querySelector('[data-toast-close]');
      if (closeButton) {
        closeButton.addEventListener('click', function () {
          dismissToast(toast);
        });
      }
    });
  }

  function applyFontScale(value) {
    var scale = ['90', '100', '110', '125'].indexOf(value) >= 0 ? value : '100';
    document.documentElement.style.fontSize = scale + '%';
    document.querySelectorAll('[data-font-scale]').forEach(function (select) {
      select.value = scale;
    });
    return scale;
  }

  function applyContrast(enabled) {
    document.documentElement.classList.toggle('high-contrast', enabled);
    document.querySelectorAll('[data-contrast-toggle]').forEach(function (toggle) {
      if (toggle.type === 'checkbox') {
        toggle.checked = enabled;
      }
      toggle.setAttribute('aria-pressed', enabled ? 'true' : 'false');
    });
    return enabled;
  }

  function initAccessibility() {
    var storedScale = storageGet(fontScaleKey) || '100';
    applyFontScale(storedScale);

    document.querySelectorAll('[data-font-scale]').forEach(function (select) {
      select.addEventListener('change', function () {
        storageSet(fontScaleKey, applyFontScale(select.value));
      });
    });

    var storedContrast = storageGet(contrastKey);
    var contrastEnabled = storedContrast === '1';
    applyContrast(contrastEnabled);

    document.querySelectorAll('[data-contrast-toggle]').forEach(function (toggle) {
      toggle.addEventListener('click', function () {
        var next = toggle.type === 'checkbox' ? toggle.checked : !document.documentElement.classList.contains('high-contrast');
        applyContrast(next);
        storageSet(contrastKey, next ? '1' : '0');
      });
    });
  }

  document.addEventListener('DOMContentLoaded', function () {
    initTheme();
    initSidebar();
    initDropdowns();
    initTabs();
    initModals();
    initPasswordToggles();
    initConfirmForms();
    initSelectAll();
    initViewToggle();
    initToasts();
    initAccessibility();
  });
})();
