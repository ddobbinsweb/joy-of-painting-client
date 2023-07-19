"use client";
import { useState } from "react";
import { ChromePicker } from "react-color";
import { useDraw } from "./hooks/useDraw";

export default function Home() {
  const [color, setColor] = useState("#000");
  const { canvasRef, onMouseDown, clear } = useDraw(drawLine);

  function drawLine({ prevPoint, currentPoint, ctx }: Draw) {
    const { x: currX, y: currY } = currentPoint;
    const lineColor = color;
    const lineWidth = 5;

    let startPoint = prevPoint ?? currentPoint;
    ctx.beginPath();
    ctx.lineWidth = lineWidth;
    ctx.strokeStyle = lineColor;
    ctx.moveTo(startPoint.x, startPoint.y);
    ctx.lineTo(currX, currY);
    ctx.stroke();

    ctx.fillStyle = lineColor;
    ctx.beginPath();
    ctx.arc(startPoint.x, startPoint.y, 2, 0, 2 * Math.PI);
    ctx.fill();
  }

  return (
    <>
      <div className="flex justify-left">
        <h1 className="text-xxl text-white">Happy Little Trees</h1>
      </div>
      <div className="w-screen h-screen bg-white flex justify-center items-center">
        <div className="flex flex-col gap-10 pr-10">
          <ChromePicker  onChange={(e: any) => setColor(e.hex)} />
          <button
            type="button"
            onClick={clear}
            className="p-2 rounded-md border border-black bg-blue text-black"
          >
            Clear
          </button>
          d
        </div>

        <canvas
          onMouseDown={onMouseDown}
          ref={canvasRef}
          width={750}
          height={750}
          className="border border-black rounded-md"
        />
      </div>
    </>
  );
}
