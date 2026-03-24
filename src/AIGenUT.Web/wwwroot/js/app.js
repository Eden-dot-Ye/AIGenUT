// AIGenUT Dashboard Application
function dashboard() {
    return {
        // State
        activeTab: 'dashboard',
        loading: true,
        analysis: {},
        coverage: {},
        stats: {},
        selectedFile: null,

        // Expression Evaluator
        evalExpression: '',
        evalVariables: '',
        evalResult: null,
        evalError: false,

        // Markdown Converter
        mdInput: '',
        mdResult: '',
        mdViewMode: 'preview',

        // CSV Parser
        csvInput: '',
        csvHasHeader: true,
        csvResult: null,

        // Tabs
        tabs: [
            { id: 'dashboard', name: 'Dashboard', icon: '<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2V6zM14 6a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2V6zM4 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2H6a2 2 0 01-2-2v-2zM14 16a2 2 0 012-2h2a2 2 0 012 2v2a2 2 0 01-2 2h-2a2 2 0 01-2-2v-2z"/></svg>' },
            { id: 'coverage', name: 'Coverage', icon: '<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"/></svg>' },
            { id: 'playground', name: 'Playground', icon: '<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z"/><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z"/></svg>' },
            { id: 'source', name: 'Source', icon: '<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4"/></svg>' },
            { id: 'tests', name: 'Tests', icon: '<svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"/></svg>' },
        ],

        // Expression examples
        exprExamples: [
            { label: '2 + 3 * 4', expr: '2 + 3 * 4', vars: '' },
            { label: 'sqrt(16)', expr: 'sqrt(16)', vars: '' },
            { label: 'PI * r^2', expr: 'PI * r^2', vars: '{"r": 5}' },
            { label: 'sin(PI/2)', expr: 'sin(PI / 2)', vars: '' },
            { label: 'max(a, b, c)', expr: 'max(a, b, c)', vars: '{"a": 10, "b": 25, "c": 15}' },
            { label: 'log(E)', expr: 'log(E)', vars: '' },
            { label: '(1+2)*(3+4)', expr: '(1+2)*(3+4)', vars: '' },
            { label: 'abs(-42)', expr: 'abs(-42)', vars: '' },
        ],

        // Computed
        get maxLines() {
            if (!this.analysis.sourceFiles) return 1;
            return Math.max(...this.analysis.sourceFiles.map(f => f.lineCount), 1);
        },

        // Methods
        async init() {
            await this.refresh();
        },

        async refresh() {
            this.loading = true;
            try {
                const [analysisResp, coverageResp, statsResp] = await Promise.all([
                    fetch('/api/analysis'),
                    fetch('/api/coverage'),
                    fetch('/api/demo/stats'),
                ]);
                this.analysis = await analysisResp.json();
                this.coverage = await coverageResp.json();
                this.stats = await statsResp.json();

                if (this.analysis.sourceFiles?.length > 0) {
                    this.selectedFile = this.analysis.sourceFiles[0];
                }

                setTimeout(() => {
                    this.renderCharts();
                }, 100);
            } catch (e) {
                console.error('Failed to load data:', e);
            } finally {
                this.loading = false;
            }
        },

        getFileCoverage(file) {
            if (!file.methods || file.methods.length === 0) return 0;
            const tested = file.methods.filter(m => m.hasTest).length;
            return (tested / file.methods.length) * 100;
        },

        async evaluateExpression() {
            try {
                let variables = {};
                if (this.evalVariables.trim()) {
                    variables = JSON.parse(this.evalVariables);
                }
                const resp = await fetch('/api/evaluate', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ expression: this.evalExpression, variables }),
                });
                const data = await resp.json();
                if (data.error) {
                    this.evalResult = data.error;
                    this.evalError = true;
                } else {
                    this.evalResult = data.result;
                    this.evalError = false;
                }
            } catch (e) {
                this.evalResult = e.message;
                this.evalError = true;
            }
        },

        async convertMarkdown() {
            try {
                const resp = await fetch('/api/markdown', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ markdown: this.mdInput }),
                });
                const data = await resp.json();
                this.mdResult = data.result || '';
            } catch (e) {
                this.mdResult = '<p class="text-red-500">Error: ' + e.message + '</p>';
            }
        },

        async parseCsv() {
            try {
                const resp = await fetch('/api/csv/parse', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ csv: this.csvInput, hasHeader: this.csvHasHeader }),
                });
                this.csvResult = await resp.json();
            } catch (e) {
                console.error('CSV parse error:', e);
            }
        },

        renderCharts() {
            this.renderCoverageChart();
            this.renderMethodsChart();
            this.renderClassCoverageChart();
        },

        renderCoverageChart() {
            const canvas = document.getElementById('coverageChart');
            if (!canvas || !this.coverage) return;

            const existing = Chart.getChart(canvas);
            if (existing) existing.destroy();

            const tested = this.coverage.testedMethods || 0;
            const untested = this.coverage.untestedMethods || 0;

            new Chart(canvas, {
                type: 'doughnut',
                data: {
                    labels: ['Tested', 'Untested'],
                    datasets: [{
                        data: [tested, untested],
                        backgroundColor: ['#22c55e', '#ef4444'],
                        borderWidth: 0,
                        cutout: '70%',
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: true,
                    plugins: {
                        legend: {
                            position: 'bottom',
                            labels: { padding: 20, usePointStyle: true, pointStyle: 'circle' }
                        }
                    }
                },
                plugins: [{
                    id: 'centerText',
                    beforeDraw(chart) {
                        const { width, height, ctx } = chart;
                        ctx.restore();
                        const total = tested + untested;
                        const pct = total > 0 ? ((tested / total) * 100).toFixed(0) : '0';
                        ctx.font = 'bold 28px Inter, sans-serif';
                        ctx.textBaseline = 'middle';
                        ctx.textAlign = 'center';
                        ctx.fillStyle = '#111827';
                        ctx.fillText(pct + '%', width / 2, height / 2 - 8);
                        ctx.font = '12px Inter, sans-serif';
                        ctx.fillStyle = '#6b7280';
                        ctx.fillText('Coverage', width / 2, height / 2 + 16);
                        ctx.save();
                    }
                }]
            });
        },

        renderMethodsChart() {
            const canvas = document.getElementById('methodsChart');
            if (!canvas || !this.analysis.sourceFiles) return;

            const existing = Chart.getChart(canvas);
            if (existing) existing.destroy();

            const labels = this.analysis.sourceFiles.map(f => f.className);
            const data = this.analysis.sourceFiles.map(f => f.methods?.length || 0);
            const colors = [
                '#6366f1', '#8b5cf6', '#a855f7', '#d946ef',
                '#ec4899', '#f43f5e', '#ef4444', '#f97316',
                '#eab308', '#22c55e', '#14b8a6', '#06b6d4',
            ];

            new Chart(canvas, {
                type: 'bar',
                data: {
                    labels,
                    datasets: [{
                        label: 'Methods',
                        data,
                        backgroundColor: colors.slice(0, labels.length),
                        borderRadius: 8,
                        borderSkipped: false,
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                    scales: {
                        y: { beginAtZero: true, ticks: { stepSize: 1 }, grid: { display: false } },
                        x: {
                            grid: { display: false },
                            ticks: {
                                maxRotation: 45,
                                font: { size: 11, family: 'JetBrains Mono' }
                            }
                        }
                    }
                }
            });
        },

        renderClassCoverageChart() {
            const canvas = document.getElementById('classCoverageChart');
            if (!canvas || !this.analysis.sourceFiles) return;

            const existing = Chart.getChart(canvas);
            if (existing) existing.destroy();

            const files = this.analysis.sourceFiles;
            const labels = files.map(f => f.className);
            const testedData = files.map(f => f.methods?.filter(m => m.hasTest).length || 0);
            const untestedData = files.map(f => f.methods?.filter(m => !m.hasTest).length || 0);

            new Chart(canvas, {
                type: 'bar',
                data: {
                    labels,
                    datasets: [
                        {
                            label: 'Tested',
                            data: testedData,
                            backgroundColor: '#22c55e',
                            borderRadius: { topLeft: 8, topRight: 8 },
                            borderSkipped: false,
                        },
                        {
                            label: 'Untested',
                            data: untestedData,
                            backgroundColor: '#fbbf24',
                            borderRadius: { topLeft: 8, topRight: 8 },
                            borderSkipped: false,
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    plugins: {
                        legend: {
                            position: 'top',
                            labels: { usePointStyle: true, pointStyle: 'circle', padding: 20 }
                        }
                    },
                    scales: {
                        y: { beginAtZero: true, stacked: true, ticks: { stepSize: 1 }, grid: { display: false } },
                        x: {
                            stacked: true,
                            grid: { display: false },
                            ticks: {
                                maxRotation: 45,
                                font: { size: 11, family: 'JetBrains Mono' }
                            }
                        }
                    }
                }
            });
        },
    };
}
