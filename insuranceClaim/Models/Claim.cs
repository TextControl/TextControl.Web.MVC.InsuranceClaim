namespace insuranceClaim.Models {
	public class Claim {
		public string name { get; set; }
		public DateTime dob { get; set; }
		public string address { get; set; }
		public string unit { get; set; }
		public string city { get; set; }
		public string state { get; set; }
		public string zip { get; set; }
		public string desc { get; set; }
	}

	public class ClaimView {
		public Claim Claim { get; set; }
		public byte[] Document { get; set; }
	}
}
