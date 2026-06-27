// ===== VSSM Dashboard Charts =====
(function () {
    'use strict';

    const GRID = 'rgba(0,0,0,0.06)';
    const TICK = '#B0B7C8';
    const GREEN = '#22C55E';
    const BLUE = '#3B82F6';

    const base = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: { display: false },
            tooltip: {
                backgroundColor: '#1C2340',
                titleColor: '#fff',
                bodyColor: 'rgba(255,255,255,0.75)',
                padding: 9,
                cornerRadius: 8,
                displayColors: false
            }
        }
    };

    const sc = {
        x: {
            display: true,
            grid: { color: GRID, drawBorder: false },
            ticks: { color: TICK, font: { size: 10 }, maxTicksLimit: 7 },
            border: { display: false }
        },
        y: {
            display: true,
            grid: { color: GRID, drawBorder: false },
            ticks: { color: TICK, font: { size: 10 }, maxTicksLimit: 4 },
            border: { display: false }
        }
    };

    // کالاهای زیر حداقل — area green
    (function () {
        const el = document.getElementById('chart-below-min');
        if (!el) return;
        const ctx = el.getContext('2d');
        const g = ctx.createLinearGradient(0, 0, 0, 120);
        g.addColorStop(0, 'rgba(34,197,94,0.22)');
        g.addColorStop(1, 'rgba(34,197,94,0)');
        new Chart(ctx, {
            type: 'line',
            data: {
                labels: ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور', 'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'],
                datasets: [{
                    data: [4, 5, 5, 6, 7, 8, 9, 10, 11, 12, 14, 16],
                    borderColor: GREEN, borderWidth: 2.5,
                    backgroundColor: g, fill: true, tension: 0.45,
                    pointRadius: 3, pointBackgroundColor: GREEN,
                    pointBorderColor: '#fff', pointBorderWidth: 2, pointHoverRadius: 5
                }]
            },
            options: { ...base, scales: { ...sc, y: { ...sc.y, min: 0 } } }
        });
    }());

    // کالاهای زیر سطح درخواست — bar blue
    (function () {
        const el = document.getElementById('chart-below-request');
        if (!el) return;
        const ctx = el.getContext('2d');
        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['۱', '۲', '۳', '۴', '۵', '۶', '۷', '۸', '۹', '۱۰', '۱۱', '۱۲', '۱۳', '۱۴'],
                datasets: [{
                    data: [6, 9, 8, 12, 9, 14, 7, 11, 8, 12, 8, 11, 7, 10],
                    backgroundColor: BLUE, hoverBackgroundColor: '#2563EB',
                    borderRadius: 5, borderSkipped: false, barPercentage: 0.65
                }]
            },
            options: {
                ...base,
                scales: {
                    x: { ...sc.x, grid: { display: false } },
                    y: { ...sc.y, min: 0 }
                }
            }
        });
    }());

    // کالاهای نزدیک به منقضی شدن — zigzag green
    (function () {
        const el = document.getElementById('chart-near-expiry');
        if (!el) return;
        const ctx = el.getContext('2d');
        const lbs = Array.from({ length: 18 }, (_, i) => String(i + 1));
        new Chart(ctx, {
            type: 'line',
            data: {
                labels: lbs,
                datasets: [{
                    data: [8, 4, 9, 4, 8, 4, 9, 3, 8, 4, 9, 4, 7, 4, 9, 3, 7, 2],
                    borderColor: GREEN, borderWidth: 2.5,
                    backgroundColor: 'transparent', fill: false, tension: 0.1,
                    pointRadius: 3, pointBackgroundColor: '#fff',
                    pointBorderColor: GREEN, pointBorderWidth: 2, pointHoverRadius: 5
                }]
            },
            options: { ...base, scales: sc }
        });
    }());

    // کالاهای بالای حداکثر — zigzag blue
    (function () {
        const el = document.getElementById('chart-above-max');
        if (!el) return;
        const ctx = el.getContext('2d');
        const lbs = Array.from({ length: 18 }, (_, i) => String(i + 1));
        new Chart(ctx, {
            type: 'line',
            data: {
                labels: lbs,
                datasets: [{
                    data: [5, 10, 4, 9, 5, 11, 4, 10, 6, 11, 5, 9, 6, 10, 5, 11, 4, 9],
                    borderColor: BLUE, borderWidth: 2.5,
                    backgroundColor: 'transparent', fill: false, tension: 0.1,
                    pointRadius: 3, pointBackgroundColor: '#fff',
                    pointBorderColor: BLUE, pointBorderWidth: 2, pointHoverRadius: 5
                }]
            },
            options: { ...base, scales: sc }
        });
    }());

}());