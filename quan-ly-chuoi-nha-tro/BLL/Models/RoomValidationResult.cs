using System.Collections.Generic;

namespace QuanLyNhaTro.BLL.Models
{
    /// <summary>
    /// Result of a validation operation
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new List<string>();
        
        public void AddError(string error)
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                Errors.Add(error);
            }
        }
        
        public string GetErrorMessage()
        {
            return string.Join("\n", Errors);
        }
    }
}
