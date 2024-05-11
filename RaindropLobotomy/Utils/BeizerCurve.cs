using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace RaindropLobotomy.Utils
{
    public class BeizerCurve
    {
        private Vector3 p0;
        private Vector3 p1;
        private Vector3 p2;
        public float totalLength;
        private const int subdivisions = 200;
        private float[] timeTable;
        private float[] distanceTable;

        public BeizerCurve() {

        }

        public BeizerCurve(Vector3 p0, Vector3 p1, Vector3 p2) {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;

            totalLength = 0f;
            float increment = 1f / subdivisions;
            timeTable = new float[subdivisions];
            distanceTable = new float[subdivisions];

            Vector3 previous = GetBeizerPoint(0);
            float interval = 0f;

            for (int i = 0; i < subdivisions; i++) {
                interval = increment * i;
                Vector3 point = GetBeizerPoint(interval);
                float dist = Vector3.Distance(point, previous);

                Debug.DrawLine(previous, point, Color.red, 60f);
                totalLength += dist;
                previous = point;
                timeTable[i] = interval;
                distanceTable[i] = totalLength;
            }
        }

        public Vector3 GetBeizerPoint(float time) {
            return Vector3.Lerp(
                Vector3.Lerp(p0, p1, time),
                Vector3.Lerp(p1, p2, time),
                time
            );
        }

        public Vector3 GetBeizerPointAtDistance(float distance) {
            float t = 0;
            for (int i = 0; i < distanceTable.Length; i++) {
                if (distanceTable[i] > distance) {
                    break;
                }

                t = timeTable[i];
            }

            return GetBeizerPoint(t);
        }

        public Vector3 GetRotationAlongCurve(float distance, float quality = 0.22f) {
            Vector3 p1 = GetBeizerPoint(distance);
            Vector3 p2 = GetBeizerPointAtDistance(distance + quality);

            return (p2 - p1).normalized;
        }
    }
}
