using insuranceClaim.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using TXTextControl;
using TXTextControl.DocumentServer;

namespace insuranceClaim.Controllers {
	public class HomeController : Controller {

		[HttpPost]
		public string ExportPDF([FromBody] TXTextControl.Web.MVC.DocumentViewer.Models.SignatureData data) {

			byte[] bPDF;

			Claim completedClaim = new Claim();

			foreach (PropertyInfo propertyInfo in completedClaim.GetType().GetProperties()) {
				var dataPoint = data.FormFields.Find(i => i.Name == propertyInfo.Name);

				if (dataPoint != null && propertyInfo.PropertyType == typeof(DateTime) &&
						DateTime.TryParse(dataPoint.Value, out var parsedDate)) {

					propertyInfo.SetValue(completedClaim, parsedDate);
				}
				else if (dataPoint != null) {
					propertyInfo.SetValue(completedClaim, dataPoint.Value);
				}
			}

			// create temporary ServerTextControl
			using (TXTextControl.ServerTextControl tx = new TXTextControl.ServerTextControl()) {
				tx.Create();

				// load the document
				tx.Load(Convert.FromBase64String(data.SignedDocument.Document),
					TXTextControl.BinaryStreamType.InternalUnicodeFormat);

				FlattenFormFields(tx);

				// save the document as PDF
				tx.Save(out bPDF, TXTextControl.BinaryStreamType.AdobePDFA);
			}

			return Convert.ToBase64String(bPDF);
		}

		private void FlattenFormFields(ServerTextControl textControl) {
			int fieldCount = textControl.FormFields.Count;

			for (int i = 0; i < fieldCount; i++) {
				TextFieldCollectionBase.TextFieldEnumerator fieldEnum =
				  textControl.FormFields.GetEnumerator();
				fieldEnum.MoveNext();

				FormField curField = (FormField)fieldEnum.Current;
				textControl.FormFields.Remove(curField, true);
			}
		}

		public IActionResult Index() {

			byte[] data;

			Claim claim = new Claim() {
				name = "Tim Typer  ",
				dob = new DateTime(2010, 5, 5),
			};

			using (TXTextControl.ServerTextControl tx = new TXTextControl.ServerTextControl()) {
				tx.Create();
				tx.Load("App_Data/claim_form.tx", TXTextControl.StreamType.InternalUnicodeFormat);

				using (MailMerge mailMerge = new MailMerge()) {
					mailMerge.TextComponent = tx;
					mailMerge.FormFieldMergeType = FormFieldMergeType.Preselect;
					mailMerge.RemoveEmptyFields = false;
					mailMerge.MergeObject(claim);
				}


				tx.Save(out data, TXTextControl.BinaryStreamType.InternalUnicodeFormat);
			}

			ClaimView cv = new ClaimView() {
				Claim = claim,
				Document = data
			};

			return View(cv);
		}


	}
}