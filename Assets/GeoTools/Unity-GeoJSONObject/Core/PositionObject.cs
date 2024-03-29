﻿using UnityEngine;
using System.Collections;


namespace GeoJSON {

	[System.Serializable]
	public class PositionObject {
		public double latitude;
		public double longitude;

		public PositionObject() {
		}

		public PositionObject(double pointLongitude, double pointLatitude) {
			this.longitude = pointLongitude;
			this.latitude = pointLatitude;
		}

		public PositionObject(JSONObject jsonObject) {
			longitude = jsonObject.list [0].f;
			latitude = jsonObject.list [1].f;
		}

		public JSONObject Serialize() {

			JSONObject jsonObject = new JSONObject (JSONObject.Type.ARRAY);
			jsonObject.Add (longitude);
			jsonObject.Add (latitude);

			return jsonObject;
		}

		override public string ToString() {
			return longitude.ToString("F4") + "," + latitude.ToString("F4");
		}

		public double[] ToArray() {

			double[] array = new double[2];

			array [0] = longitude;
			array [1] = latitude;

			return array;
		}
	}
}
