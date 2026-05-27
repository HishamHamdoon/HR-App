/*
 * Lightweight Hijri (Umm al-Qura) date picker — no external dependencies.
 * Conversions use the browser's Intl Islamic-Umalqura calendar. The visible field shows
 * a Hijri date; the original input keeps a Gregorian "yyyy-MM-dd" value for the server.
 */
(function () {
    'use strict';

    var monthNames = ['محرم', 'صفر', 'ربيع الأول', 'ربيع الآخر', 'جمادى الأولى', 'جمادى الآخرة',
        'رجب', 'شعبان', 'رمضان', 'شوال', 'ذو القعدة', 'ذو الحجة'];
    var weekDays = ['أحد', 'اثنين', 'ثلاثاء', 'أربعاء', 'خميس', 'جمعة', 'سبت']; // index = getUTCDay (0=Sun)

    var hijriFmt = new Intl.DateTimeFormat('en-US-u-ca-islamic-umalqura',
        { day: 'numeric', month: 'numeric', year: 'numeric' });

    function toParts(date) {
        var o = {};
        hijriFmt.formatToParts(date).forEach(function (p) {
            if (p.type === 'day') o.hd = +p.value;
            else if (p.type === 'month') o.hm = +p.value;
            else if (p.type === 'year') o.hy = +p.value;
        });
        return o;
    }

    // Hijri (y,m,d) -> Gregorian Date (UTC). Walks from a close approximation; the Hijri
    // composite key increases monotonically with the date, so this always converges.
    function fromHijri(hy, hm, hd) {
        var gy = Math.floor(hy * 0.970224 + 621.5);
        var d = new Date(Date.UTC(gy, 0, 1));
        var target = hy * 10000 + hm * 100 + hd;
        for (var i = 0; i < 2000; i++) {
            var p = toParts(d);
            var cur = p.hy * 10000 + p.hm * 100 + p.hd;
            if (cur === target) return d;
            d.setUTCDate(d.getUTCDate() + (target > cur ? 1 : -1));
        }
        return d;
    }

    function monthLength(hy, hm) {
        var first = fromHijri(hy, hm, 1);
        var probe = new Date(first);
        probe.setUTCDate(probe.getUTCDate() + 29);
        return toParts(probe).hm === hm ? 30 : 29;
    }

    function pad(n) { return (n < 10 ? '0' : '') + n; }
    function toGregStr(d) { return d.getUTCFullYear() + '-' + pad(d.getUTCMonth() + 1) + '-' + pad(d.getUTCDate()); }
    function toHijriStr(d) { var p = toParts(d); return p.hd + ' ' + monthNames[p.hm - 1] + ' ' + p.hy + ' هـ'; }

    function injectStyles() {
        if (document.getElementById('hijri-pop-style')) return;
        var s = document.createElement('style');
        s.id = 'hijri-pop-style';
        s.textContent =
            '.hijri-pop{position:absolute;z-index:1056;display:none;background:var(--bs-card-bg,#fff);' +
            'border:1px solid var(--bs-border-color,#ddd);border-radius:.5rem;box-shadow:0 .5rem 1.5rem rgba(0,0,0,.2);padding:.6rem;min-width:18rem;}' +
            '.hijri-grid{display:grid;grid-template-columns:repeat(7,1fr);gap:2px;text-align:center;}' +
            '.hijri-dow{font-size:.7rem;color:var(--bs-secondary-color,#888);padding:.25rem 0;}' +
            '.hijri-day{padding:.4rem 0;border-radius:.375rem;cursor:pointer;}' +
            '.hijri-day:hover{background:var(--bs-secondary-bg,#eee);}' +
            '.hijri-day.selected{background:var(--bs-primary,#696cff);color:#fff;}' +
            '.hijri-day.empty{visibility:hidden;cursor:default;}';
        document.head.appendChild(s);
    }

    function attach(input) {
        injectStyles();
        input.type = 'hidden';

        var group = document.createElement('div');
        group.className = 'input-group';
        input.parentNode.insertBefore(group, input);

        var icon = document.createElement('span');
        icon.className = 'input-group-text cursor-pointer';
        icon.innerHTML = '<i class="icon-base ti tabler-calendar"></i>';

        var visible = document.createElement('input');
        visible.type = 'text';
        visible.readOnly = true;
        visible.className = 'form-control';
        visible.style.cursor = 'pointer';
        visible.placeholder = '—';

        group.appendChild(icon);
        group.appendChild(visible);
        group.appendChild(input);

        var pop = document.createElement('div');
        pop.className = 'hijri-pop';
        document.body.appendChild(pop);

        var selected = null;
        var view = null; // { hy, hm }

        if (input.value) {
            var v = input.value.split('-');
            if (v.length === 3) selected = new Date(Date.UTC(+v[0], +v[1] - 1, +v[2]));
        }
        function syncVisible() { visible.value = selected ? toHijriStr(selected) : ''; }
        syncVisible();

        function render() {
            var len = monthLength(view.hy, view.hm);
            var first = fromHijri(view.hy, view.hm, 1);
            var offset = first.getUTCDay();
            var selKey = selected ? (function () { var p = toParts(selected); return p.hy + '-' + p.hm + '-' + p.hd; })() : '';

            var html = '<div class="d-flex align-items-center justify-content-between mb-2">' +
                '<button type="button" class="btn btn-sm btn-icon btn-text-secondary" data-nav="-1"><i class="icon-base ti tabler-chevron-right"></i></button>' +
                '<strong>' + monthNames[view.hm - 1] + ' ' + view.hy + '</strong>' +
                '<button type="button" class="btn btn-sm btn-icon btn-text-secondary" data-nav="1"><i class="icon-base ti tabler-chevron-left"></i></button>' +
                '</div><div class="hijri-grid">';
            for (var w = 0; w < 7; w++) html += '<div class="hijri-dow">' + weekDays[w] + '</div>';
            for (var e = 0; e < offset; e++) html += '<div class="hijri-day empty"></div>';
            for (var d = 1; d <= len; d++) {
                var isSel = selKey === (view.hy + '-' + view.hm + '-' + d);
                html += '<div class="hijri-day' + (isSel ? ' selected' : '') + '" data-day="' + d + '">' + d + '</div>';
            }
            html += '</div>';
            pop.innerHTML = html;

            pop.querySelectorAll('[data-nav]').forEach(function (btn) {
                btn.addEventListener('click', function () {
                    var dir = +btn.getAttribute('data-nav');
                    view.hm += dir;
                    if (view.hm > 12) { view.hm = 1; view.hy++; }
                    else if (view.hm < 1) { view.hm = 12; view.hy--; }
                    render();
                });
            });
            pop.querySelectorAll('.hijri-day[data-day]').forEach(function (cell) {
                cell.addEventListener('click', function () {
                    var hd = +cell.getAttribute('data-day');
                    selected = fromHijri(view.hy, view.hm, hd);
                    input.value = toGregStr(selected);
                    input.dispatchEvent(new Event('change', { bubbles: true }));
                    syncVisible();
                    hide();
                });
            });
        }

        function position() {
            var r = visible.getBoundingClientRect();
            pop.style.top = (window.scrollY + r.bottom + 4) + 'px';
            pop.style.left = (window.scrollX + r.left) + 'px';
        }
        function open() {
            var ref = selected || new Date();
            var p = toParts(ref);
            view = { hy: p.hy, hm: p.hm };
            render();
            pop.style.display = 'block';
            position();
        }
        function hide() { pop.style.display = 'none'; }

        visible.addEventListener('click', open);
        icon.addEventListener('click', open);
        document.addEventListener('click', function (e) {
            if (e.target !== visible && e.target !== icon && !pop.contains(e.target) && !icon.contains(e.target)) hide();
        });
    }

    window.HijriPicker = { attach: attach };
})();
