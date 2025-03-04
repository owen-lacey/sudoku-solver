import Webcam from "react-webcam";
import {useRef} from "react";
import "../styles/detect-sudoku.scss";

function DetectSudoku() {
    const webcamRef = useRef(null);
    const canvasRef = useRef(null);
  return (
    <div className="detect-sudoku">
        <Webcam
            ref={webcamRef}
            muted={true}
            style={{
            }}
        />

        <canvas
            ref={canvasRef}
            style={{
                width: webcamRef.current?.video.videoWidth,
                height: webcamRef.current?.video.videoHeight
            }}
        />
    </div>
  );
}

export default DetectSudoku;