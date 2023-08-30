"use client";
import {
  AppBar,
  Button,
  Card,
  CardContent,
  Container,
  List,
  ListItem,
  Toolbar,
  Typography
} from "@mui/material";
import { useSearchParams } from "next/navigation";
import { useState } from "react";
import { ChromePicker } from "react-color";
import { useDraw } from "./hooks/useDraw";

export default function Home() {
  const [color, setColor] = useState("#22194D");
  const { canvasRef, onMouseDown, clear } = useDraw(drawLine);

  const searchParams = useSearchParams();
  const key = searchParams.get("key");

  function drawLine({ prevPoint, currentPoint, ctx }: Draw) {
    const { x: currX, y: currY } = currentPoint;
    const lineColor = color;
    const lineWidth = 5;
    console.log(color);
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
  const submit = () => {
    if (!canvasRef.current) return;

    const canvas = canvasRef.current;
    // Convert canvas data to data URL
    const dataURL = canvas.toDataURL("image/png");

    // Create an image element
    const img = new Image();
    img.src = dataURL;

    // Wait for the image to load before continuing
    img.onload = () => {
      // Create a new canvas to draw the image
      const newCanvas = document.createElement("canvas");
      newCanvas.width = img.width;
      newCanvas.height = img.height;

      // Get the context of the new canvas
      const newCtx = newCanvas.getContext("2d");
      if (newCtx) {
        newCtx.drawImage(img, 0, 0);
        // TODO : do this mo better
        let api= '';
        if(key){
           api = key;
        }
        
        // Get the image data from the new canvas in a format that can be submitted to the API (e.g., Blob)
        newCanvas.toBlob((blob) => {
          if (blob) {
            // Now you can submit the 'blob' to your API using XMLHttpRequest, fetch, or any other method you prefer
            // For example, if you're using fetch:
            fetch("https://api.jop.revunit.com/original", {
              method: "POST",
              body: blob,
              headers: {
                "X-API-KEY": api,
                "Content-Type": "image/jpeg", // adjust the content type based on the image format you are using (e.g., 'image/jpeg' for JPEG)
              },
            })
              .then((response) => {
                // Handle the API response here
                console.log("API Response:", response);
              })
              .catch((error) => {
                // Handle errors here
                console.error("Error:", error);
              });
          }
        });
      }
    };
  };

  const changeColor = (e:any)=>{
    setColor(e.hex);
  }

  return (
    <>
      {/* Nav Bar */}
      <AppBar position="static">
        <Container maxWidth="xl">
          <Toolbar>
              <Typography variant="h5" component="div" sx={{ flexGrow: 1 }}>
                Happy Little Trees
              </Typography>
        
              <Typography variant="h6" component="div" sx={{ flexGrow: 1 }}>
                Api key: {key}
              </Typography>
         
          </Toolbar>
        </Container>
      </AppBar>

      <main>
        {/* Left Column */}
        <div className=" bg-gradient-to-r from-cyan-500 to-blue-500 flex justify-center items-center p-5">
          <div className="flex flex-col gap-10 pr-10">
            <ChromePicker color={color} onChange={changeColor} />
            <Button onClick={submit} variant="contained">
              Submit
            </Button>
            <Button onClick={clear} variant="contained">
              Clear
            </Button>
          </div>
          {/* center column */}
          <div className="flex flex-col gap-10 rounded-md bg-white">
            <canvas
              onMouseDown={onMouseDown}
              ref={canvasRef}
              width={750}
              height={750}
              className="border border-black rounded-md"
            />
          </div>
          {/* center column */}
          <div className="flex flex-col gap-10 pl-10">
            <Card sx={{ minWidth: 275 }}>
              <CardContent>
                <Typography variant="h5" component="div">
                  How to use
                </Typography>
                <List>
                  <ListItem>click and drag the mouse</ListItem>
                  <ListItem>when done click submit</ListItem>
                </List>
              </CardContent>
            </Card>
          </div>
        </div>
      </main>
    </>
  );
}
