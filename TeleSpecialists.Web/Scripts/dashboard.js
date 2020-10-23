(function($) {
  'use strict';
  $(function() {
    if ($("#curved-line-chart").length) {
      var d1 = [
        [0, 6],
        [1, 14],
        [2, 10],
        [3, 14],
        [4, 5]
      ];
      var d2 = [
        [0, 6],
        [1, 7],
        [2, 11],
        [3, 8],
        [4, 11]
      ];
      var d3 = [
        [0, 6],
        [1, 5],
        [2, 6],
        [3, 10],
        [4, 5]
      ];
      var d4 = [
        [0, 2],
        [1, 7],
        [2, 8],
        [3, 6],
        [4, 7]
      ];

      var curvedLineOptions = {
        series: {
          shadowSize: 0,
          curvedLines: { //This is a third party plugin to make curved lines
            apply: true,
            active: true,
            monotonicFit: true
          },
          lines: {
            show: false,
            lineWidth: 0
          }
        },
        grid: {
          borderWidth: 0,
          labelMargin: 10,
          hoverable: true,
          clickable: true,
          mouseActiveRadius: 6
        },
        xaxis: {
          tickDecimals: 0,
          ticks: false
        },
        yaxis: {
          tickDecimals: 0,
          ticks: false
        },
        legend: {
          noColumns: 5,
          container: $("#chartLegend")
        }
      }

      $.plot($("#curved-line-chart"), [{
          data: d1,
          lines: {
            show: true,
            fill: 0.98
          },
          label: 'Stroke Alert',
          stack: true,
          color: '#fd5356'
        },
        {
          data: d2,
          lines: {
            show: true,
            fill: 0.98
          },
          label: 'Stat EEG',
          stack: true,
          color: '#fe9091'
        },
        {
          data: d3,
          lines: {
            show: true,
            fill: 0.98
          },
          label: 'Routine Consult',
          stack: true,
          color: '#feafb0'
        },
        {
            data: d4,
            lines: {
                show: true,
                fill: 0.56
            },
            label: 'Stat Consult',
            stack: true,
            color: '#fedbb0'
        }
      ], curvedLineOptions);
        
    }
    if ($(".datepicker").length) {
      $('.datepicker').datepicker({
        enableOnReadonly: true,
        todayHighlight: true,
      });
    }

/*

      
    if ($('#ct-chart-vartical-stacked-bar').length) {
      new Chartist.Bar('#ct-chart-vartical-stacked-bar', {
          labels: [ 'Open', 'Waiting to Accept', 'Accepted', 'Complete'],
        series: [{
            //"name": "",
            "data": [8, 4, 6, 3]
        }
          //  ,
          //{
          //  "name": "Stat EEG",
          //  "data": [2, 7, 4, 8, 4, 6, 1]
          //},
          //{
          //  "name": "Routine Consult",
          //  "data": [4, 3, 3, 6, 7, 2, 4]
          //}
        ]
      }, {
        seriesBarDistance: 10,
        //reverseData: true,
        horizontalBars: false,
        height: '280px',
        fullWidth: true,
        chartPadding: {
          top: 30,
          left: 0,
          right: 0,
          bottom: 0
        },
        plugins: [
          Chartist.plugins.legend()
        ]
      });
    }
   


    if ($("#c3-pie-chart").length) {
      var c3PieChart = c3.generate({
        bindto: '#c3-pie-chart',
        data: {
          // iris data from R
          columns: [],
          type: 'pie',
          onclick: function(d, i) {
            console.log("onclick", d, i);
          },
          onmouseover: function(d, i) {
            console.log("onmouseover", d, i);
          },
          onmouseout: function(d, i) {
            console.log("onmouseout", d, i);
          }
        },
        color: {
          pattern: ['#009698', '#00fcff', '#24d5d8']
        },
        padding: {
          top: 0,
          right: 0,
          bottom: 30,
          left: 0,
        }
      });
      setTimeout(function() {
        c3PieChart.load({
          columns: [
            ["Income", 4.2, 2.2, 1.2, 1.2, 0.2, 0.4, 0.3, 0.2, 0.2, 0.1, 0.2, 0.2, 0.1, 0.1, 0.2, 0.4, 0.4, 0.3, 0.3],
            ["Outcome", 0.3, 1.5, 1.0, 1.3, 0.5, 0.3, 1.6, 1.0, 1.3, 1.4, 0.8, 1.5, 1.0, 0.4, 1.3, 1.4, 1.5, 1.0, 1.5],
            ["Revenue", 2.5, 1.9, 5.1, 8.8, 4.2, 9.1, 1.7, 1.8, 1.0, 2.5, 2.0, 2.3, 2.1, 2.0, 2.4, 2.3, 1.8, 2.2, 2.3],
          ]
        });
      }, 100);
    }
    if ($("#dashboard-vmap").length) {
      $('#dashboard-vmap').vectorMap({
        map: 'world_mill_en',
        panOnDrag: true,
        backgroundColor: 'transparent',
        focusOn: {
          x: 0.5,
          y: 0.5,
          scale: 1,
          animate: true
        },
        series: {
          regions: [{
            scale: ['#e6edf2'],
            normalizeFunction: 'polynomial',
            values: {
              "AF": 16.63,
              "AL": 11.58,
              "DZ": 158.97,
              "AO": 85.81,
              "AG": 1.1,
              "AR": 351.02,
              "AM": 8.83,
              "AU": 1219.72,
              "AT": 366.26,
              "AZ": 52.17,
              "BS": 7.54,
              "BH": 21.73,
              "BD": 105.4,
              "BB": 3.96,
              "BY": 52.89,
              "BE": 461.33,
              "BZ": 1.43,
              "BJ": 6.49,
              "BT": 1.4,
              "BO": 19.18,
              "BA": 16.2,
              "BW": 12.5,
              "BR": 2023.53,
              "BN": 11.96,
              "BG": 44.84,
              "BF": 8.67,
              "BI": 1.47,
              "KH": 11.36,
              "CM": 21.88,
              "CA": 1563.66,
              "CV": 1.57,
              "CF": 2.11,
              "TD": 7.59,
              "CL": 199.18,
              "CN": 5745.13,
              "CO": 283.11,
              "KM": 0.56,
              "CD": 12.6,
              "CG": 11.88,
              "CR": 35.02,
              "CI": 22.38,
              "HR": 59.92,
              "CY": 22.75,
              "CZ": 195.23,
              "DK": 304.56,
              "DJ": 1.14,
              "DM": 0.38,
              "DO": 50.87,
              "EC": 61.49,
              "EG": 216.83,
              "SV": 21.8,
              "GQ": 14.55,
              "ER": 2.25,
              "EE": 19.22,
              "ET": 30.94,
              "FJ": 3.15,
              "FI": 231.98,
              "FR": 2555.44,
              "GA": 12.56,
              "GM": 1.04,
              "GE": 11.23,
              "DE": 3305.9,
              "GH": 18.06,
              "GR": 305.01,
              "GD": 0.65,
              "GT": 40.77,
              "GN": 4.34,
              "GW": 0.83,
              "GY": 2.2,
              "HT": 6.5,
              "HN": 15.34,
              "HK": 226.49,
              "HU": 132.28,
              "IS": 12.77,
              "IN": 1430.02,
              "ID": 695.06,
              "IR": 337.9,
              "IQ": 84.14,
              "IE": 204.14,
              "IL": 201.25,
              "IT": 2036.69,
              "JM": 13.74,
              "JP": 5390.9,
              "JO": 27.13,
              "KZ": 129.76,
              "KE": 32.42,
              "KI": 0.15,
              "KR": 986.26,
              "KW": 117.32,
              "KG": 4.44,
              "LA": 6.34,
              "LV": 23.39,
              "LB": 39.15,
              "LS": 1.8,
              "LR": 0.98,
              "LY": 77.91,
              "LT": 35.73,
              "LU": 52.43,
              "MK": 9.58,
              "MG": 8.33,
              "MW": 5.04,
              "MY": 218.95,
              "MV": 1.43,
              "ML": 9.08,
              "MT": 7.8,
              "MR": 3.49,
              "MU": 9.43,
              "MX": 1004.04,
              "MD": 5.36,
              "MN": 5.81,
              "ME": 3.88,
              "MA": 91.7,
              "MZ": 10.21,
              "MM": 35.65,
              "NA": 11.45,
              "NP": 15.11,
              "NL": 770.31,
              "NZ": 138,
              "NI": 6.38,
              "NE": 5.6,
              "NG": 206.66,
              "NO": 413.51,
              "OM": 53.78,
              "PK": 174.79,
              "PA": 27.2,
              "PG": 8.81,
              "PY": 17.17,
              "PE": 153.55,
              "PH": 189.06,
              "PL": 438.88,
              "PT": 223.7,
              "QA": 126.52,
              "RO": 158.39,
              "RU": 1476.91,
              "RW": 5.69,
              "WS": 0.55,
              "ST": 0.19,
              "SA": 434.44,
              "SN": 12.66,
              "RS": 38.92,
              "SC": 0.92,
              "SL": 1.9,
              "SG": 217.38,
              "SK": 86.26,
              "SI": 46.44,
              "SB": 0.67,
              "ZA": 354.41,
              "ES": 1374.78,
              "LK": 48.24,
              "KN": 0.56,
              "LC": 1,
              "VC": 0.58,
              "SD": 65.93,
              "SR": 3.3,
              "SZ": 3.17,
              "SE": 444.59,
              "CH": 522.44,
              "SY": 59.63,
              "TW": 426.98,
              "TJ": 5.58,
              "TZ": 22.43,
              "TH": 312.61,
              "TL": 0.62,
              "TG": 3.07,
              "TO": 0.3,
              "TT": 21.2,
              "TN": 43.86,
              "TR": 729.05,
              "TM": 0,
              "UG": 17.12,
              "UA": 136.56,
              "AE": 239.65,
              "GB": 2258.57,
              "US": 14624.18,
              "UY": 40.71,
              "UZ": 37.72,
              "VU": 0.72,
              "VE": 285.21,
              "VN": 101.99,
              "YE": 30.02,
              "ZM": 15.69,
              "ZW": 5.57
            }
          }]
        }
      });
    }

    */


    if ($("#YearlyProgress").length) {
      var bar = new ProgressBar.Circle(YearlyProgress, {
        color: '#000',
        strokeWidth: 12,
        trailWidth: 12,
        trailColor: '#f5f5f5',
        easing: 'easeInOut',
        duration: 1400,
        text: {
          autoStyleContainer: false
        },
        from: {
          color: '#7900e3',
          width: 12
        },
        to: {
          color: '#7900e3',
          width: 12
        },
        // Set default step function for all animate calls
        step: function(state, circle) {
          circle.path.setAttribute('stroke', state.color);
          circle.path.setAttribute('stroke-width', state.width);

          var value = Math.round(circle.value() * 100);
          if (value === 0) {
            circle.setText('');
          } else {
            circle.setText(value);
          }

        }
      });
      bar.text.style.fontSize = '1.5rem';
      bar.animate(0.4); // Number from 0.0 to 1.0
    }
    if ($("#MonthlyProgress").length) {
      var bar = new ProgressBar.Circle(MonthlyProgress, {
        color: '#000',
        strokeWidth: 12,
        trailWidth: 12,
        trailColor: '#f5f5f5',
        easing: 'easeInOut',
        duration: 1400,
        text: {
          autoStyleContainer: false
        },
        from: {
          color: '#ff003e',
          width: 12
        },
        to: {
          color: '#ff003e',
          width: 12
        },
        // Set default step function for all animate calls
        step: function(state, circle) {
          circle.path.setAttribute('stroke', state.color);
          circle.path.setAttribute('stroke-width', state.width);

          var value = Math.round(circle.value() * 100);
          if (value === 0) {
            circle.setText('');
          } else {
            circle.setText(value);
          }

        }
      });
      bar.text.style.fontSize = '1.5rem';
      bar.animate(0.4); // Number from 0.0 to 1.0
    }
  });
})(jQuery);