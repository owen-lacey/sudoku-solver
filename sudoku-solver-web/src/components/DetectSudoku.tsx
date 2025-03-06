import Webcam from "react-webcam";
import {useCallback, useEffect, useRef, useState} from "react";
import "../styles/detect-sudoku.scss";
import '@tensorflow/tfjs-backend-cpu';
import '@tensorflow/tfjs-backend-webgl';
import * as cocossd from '@tensorflow-models/coco-ssd';
import {ObjectDetection} from "@tensorflow-models/coco-ssd";
import axios from "axios";
import {DetectedSudoku} from "src/models/detected-sudoku.ts";

function DetectSudoku() {
    const webcamRef = useRef<Webcam>(null);
    const canvasRef = useRef<HTMLCanvasElement>(null);
    const [image, setImage] = useState<string | null>(null);

    const takePhoto = useCallback(() => {
        // take a photo from the webcam image
        if (webcamRef.current) {
            const imageSrc = webcamRef.current.getScreenshot();
            setImage(imageSrc);
        }
    }, [webcamRef]);

    const checkForSudoku = async (imageContent: string) => {
        const formData = new FormData();
        formData.append("uploadedImage", imageContent);
        const result = await axios.post<DetectedSudoku>("http://localhost:5166/detect-sudoku", formData,
            {
                headers: {
                    "content-type": "multipart/form-data",
                }
            });

        const ctx = canvasRef.current!.getContext("2d")!;
        ctx.strokeStyle = "red";
        ctx.fillStyle = "red";
        ctx.beginPath();
        ctx.rect(
            result.data.box[0] * canvasRef.current!.width,// x = left
            result.data.box[1] * canvasRef.current!.height, // y = top
            (result.data.box[2] - result.data.box[0]) * canvasRef.current!.width, // width = right - left
            (result.data.box[3] - result.data.box[1]) * canvasRef.current!.height); // height = bottom - top
        ctx.stroke();
    };

    useEffect(() => {
        if (image && image.length > 0) {
            checkForSudoku(image!);
        }
    }, [image]);

    return (
        <div className="detect-sudoku">

            {image
                ? <img src={image} alt="webcam"/>
                : (<>
                    <Webcam
                        ref={webcamRef}
                        muted={true}
                    />
                    <div className="take-photo">
                        <button onClick={takePhoto}>Take Photo</button>
                    </div>
                </>)}

            <canvas
                ref={canvasRef}
            />
        </div>
    );
}

export default DetectSudoku;