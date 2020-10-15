using System;

namespace WolfeReiter.Identity.DualStack.Models
{
    public class CheckboxItem<T> where T : new()
    {
        public CheckboxItem()
        {
            Id = new T();
            Text = "";
            Checked = false;
        }
        public T Id { get; set; }
        public string Text { get; set; }
        public bool Checked { get; set; }
    }
}