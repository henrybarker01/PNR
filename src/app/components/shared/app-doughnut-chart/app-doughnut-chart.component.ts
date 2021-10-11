import { Component, Input, OnInit, ViewChild } from '@angular/core';
import {
  ApexChart,
  ApexDataLabels,
  ApexLegend,
  ApexNonAxisChartSeries,
  ApexPlotOptions,
  ChartComponent,
} from 'ng-apexcharts';

export type ChartOptions = {
  series: ApexNonAxisChartSeries;
  chart: ApexChart;
  labels: any;
  colors?: string[];
  plotOptions: ApexPlotOptions;
  legend?: ApexLegend;
  dataLabels: ApexDataLabels;
};

@Component({
  selector: 'app-doughnut-chart',
  templateUrl: './app-doughnut-chart.component.html',
  styleUrls: ['./app-doughnut-chart.component.scss'],
})
export class AppDoughnutChartComponent implements OnInit {
  @Input() width: string | number = '500px';
  @Input() height: string | number = '500px';
  @Input() dataSeries: number[] = [];
  @Input() dataLabels: string[] = [];
  @Input() dataLabelsFontSize = '13px';
  @Input() legendMargin: { vertical: number; horizontal: number } = {
    vertical: 5,
    horizontal: 8,
  };

  @ViewChild('chart') chart!: ChartComponent;
  public chartOptions!: ChartOptions;

  constructor() {}

  ngOnInit(): void {
    this.initChart();
  }

  private initChart(): void {
    this.chartOptions = {
      legend: {
        markers: {
          width: 0,
        },
        show: true,
        formatter: function (w, p) {
          return `
        <div style="width: 235px;background-color:#f4f4f4;height: 50px;text-align: center;font-weight: 700;font-size:14px;">
        <div style="height: 100%;text-align: center;padding-top: 17px;">${p.w.globals.series[p.seriesIndex] + ' - ' + w}</div>
        </div>`;
        },
      },
      colors: ['#18173e', '#d70000'],
      dataLabels: {
        enabled: false,
      },
      series: this.dataSeries,
      chart: {
        type: 'donut',
        width: 500,
        height: 500,
      },
      labels: ['rules applied', 'no rules applied'],
      plotOptions: {
        pie: {
          expandOnClick: false,
          donut: {
            labels: {
              show: true,
              name: {
                show: false,
              },
              total: {
                show: true,
                showAlways: true,
                formatter: function (w) {
                  return w.globals.seriesTotals.reduce((a, b) => {
                    return a + b;
                  }, 0);
                },
              },
            },
          },
        },
      },
    };
  }
}
