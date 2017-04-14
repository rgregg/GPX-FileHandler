using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCO365Demo.Models
{
    public class GPXFileViewModel
    {
        public ActivationParameters ActivationParameters { get; set; }

        public string ErrorMessage { get; set; }

        public List<GeoCoordinate> Coordinates { get; set; }

        public string Title { get; set; }

        public string SignedInUserName { get; set; }

        public bool ReadOnly { get; set; }
        
        public GPXFileViewModel(ActivationParameters parameters)
        {
            this.ActivationParameters = parameters;
        }

        public static GPXFileViewModel GetErrorModel(ActivationParameters parameters, string errorMessage)
        {
            return new GPXFileViewModel(parameters)
            {
                ErrorMessage = errorMessage,
                ReadOnly = true
            };
        }

        public static GPXFileViewModel GetErrorModel(ActivationParameters parameters, Exception ex)
        {
            return new GPXFileViewModel(parameters)
            {
                ErrorMessage = ex.Message,
                ReadOnly = true
            };

        }
    }
}