using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace MvvmTools.Web.Extensions
{
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Generates a label with a checkbox inside.
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="htmlHelper"></param>
        /// <param name="expression"></param>
        /// <param name="labelHtmlAttributes"></param>
        /// <param name="checkBoxHtmlAttributes"></param>
        /// <returns></returns>
        public static MvcHtmlString CheckBoxAndLabelFor<TModel>(this HtmlHelper<TModel> htmlHelper,
            Expression<Func<TModel, bool>> expression, object labelHtmlAttributes = null, object checkBoxHtmlAttributes = null, string labelText = null)
        {
            // If no checkbox html attributes are provided, a margin is inserted between
            // the box and the label.  I have no idea why putting a right margin on the 
            // checkbox has the desired effect, but not much about HTML makes sense to me.
            var defaultCheckboxAttributes = new {style = "margin-right:5px;"};
            
            var cb = htmlHelper.CheckBoxFor(expression, checkBoxHtmlAttributes ?? defaultCheckboxAttributes).ToHtmlString();
            var lbl = htmlHelper.LabelFor(expression, labelText, labelHtmlAttributes).ToHtmlString();

            var gtPos = lbl.IndexOf(">", StringComparison.Ordinal);
            var firstPart = lbl.Substring(0, gtPos + 1);
            var lastPart = lbl.Substring(gtPos + 1);
            var newLbl = firstPart + cb + lastPart;

            var rval = new MvcHtmlString(newLbl);
            return rval;
        }
    }
}
