import React from "react";
import { Colors } from "@/interfaces/Colors";
import { defaultColor } from "./colors";

interface IProps {
	colors?: Colors;
}

const Shirt = ({ colors = defaultColor }: IProps = {}) => {
	const [color1, color2, color3] = colors;

	return (
		<g id="shirt">
			<path
				fill={color1}
				d="M570.94,430V420h-10V410h-10v20h-10v10h-10v10h-10v10h-10V410h-10V360h-10V330h-10V320h-10V310h-20V300h-10V290h-30v10h-10V290h-20V280h-10V270h-10v20h-10v10h-20v10h-20v10h-10v10h-10v10h-10v20h-10V580h170V570h30V540h-10V450h10v30h10v10h10v10h10v10h10v10h10V510h10V500h10V490h10V480h10V470h10V460h10V430Zm-250,50v60h-10V470h10Z"
			/>
			<polygon
				fill={color2}
				points="570.94 430 560.94 430 560.94 440 550.94 440 550.94 450 550.94 460 540.94 460 540.94 470 530.94 470 530.94 480 520.94 480 520.94 490 510.94 490 510.94 480 500.94 480 500.94 470 500.94 460 490.94 460 490.94 450 490.94 440 490.94 430 490.94 420 480.94 420 480.94 410 480.94 400 480.94 390 480.94 380 480.94 370 470.94 370 470.94 380 470.94 390 470.94 400 470.94 410 470.94 420 470.94 430 480.94 430 480.94 440 480.94 450 480.94 460 480.94 470 490.94 470 490.94 480 490.94 490 500.94 490 500.94 500 510.94 500 510.94 510 520.94 510 530.94 510 530.94 500 540.94 500 540.94 490 550.94 490 550.94 480 560.94 480 560.94 470 570.94 470 570.94 460 580.94 460 580.94 450 580.94 440 580.94 430 570.94 430"
			/>
			<polygon
				fill={color2}
				points="470.94 350 460.94 350 460.94 360 460.94 370 470.94 370 470.94 360 470.94 350"
			/>
			<rect fill={color2} x="440.94" y="310" width="10" height="10" />
			<polygon
				fill={color2}
				points="370.94 290 370.94 300 380.94 300 380.94 290 380.94 280 370.94 280 370.94 290"
			/>
			<polygon
				fill={color2}
				points="360.94 440 360.94 430 360.94 420 350.94 420 350.94 410 350.94 400 350.94 390 350.94 380 340.94 380 330.94 380 330.94 390 330.94 400 330.94 410 340.94 410 340.94 420 340.94 430 330.94 430 330.94 440 330.94 450 330.94 460 340.94 460 340.94 470 340.94 480 340.94 490 340.94 500 340.94 510 340.94 520 340.94 530 330.94 530 330.94 540 330.94 550 330.94 560 330.94 570 330.94 580 340.94 580 350.94 580 360.94 580 370.94 580 370.94 570 370.94 560 370.94 550 370.94 540 370.94 530 370.94 520 370.94 510 370.94 500 370.94 490 370.94 480 370.94 470 370.94 460 370.94 450 360.94 450 360.94 440"
			/>
			<polygon
				fill={color2}
				points="340.94 330 340.94 320 350.94 320 360.94 320 360.94 310 350.94 310 340.94 310 330.94 310 330.94 320 320.94 320 310.94 320 310.94 330 310.94 340 300.94 340 300.94 350 290.94 350 290.94 360 290.94 370 280.94 370 280.94 380 280.94 390 280.94 400 280.94 410 280.94 420 280.94 430 280.94 440 280.94 450 280.94 460 280.94 470 280.94 480 280.94 490 280.94 500 280.94 510 280.94 520 280.94 530 280.94 540 280.94 550 280.94 560 280.94 570 280.94 580 290.94 580 290.94 570 290.94 560 290.94 550 290.94 540 290.94 530 290.94 520 290.94 510 290.94 500 290.94 490 290.94 480 290.94 470 290.94 460 290.94 450 290.94 440 290.94 430 290.94 420 290.94 410 290.94 400 300.94 400 300.94 390 300.94 380 300.94 370 310.94 370 310.94 360 310.94 350 320.94 350 320.94 340 320.94 330 330.94 330 340.94 330"
			/>
			<polygon
				fill={color3}
				points="550.94 410 550.94 420 550.94 430 560.94 430 560.94 420 560.94 410 550.94 410"
			/>
			<rect fill={color3} x="540.94" y="430" width="10" height="10" />
			<rect fill={color3} x="530.94" y="440" width="10" height="10" />
			<rect fill={color3} x="520.94" y="450" width="10" height="10" />
			<rect fill={color3} x="510.94" y="510" width="10" height="10" />
			<rect fill={color3} x="510.94" y="460" width="10" height="10" />
			<rect fill={color3} x="500.94" y="500" width="10" height="10" />
			<rect fill={color3} x="490.94" y="490" width="10" height="10" />
			<polygon
				fill={color3}
				points="490.94 470 480.94 470 480.94 460 480.94 450 480.94 440 480.94 430 470.94 430 470.94 420 470.94 410 470.94 400 470.94 390 470.94 380 470.94 370 460.94 370 460.94 380 460.94 390 460.94 400 460.94 410 460.94 420 460.94 430 460.94 440 460.94 450 470.94 450 470.94 460 470.94 470 470.94 480 480.94 480 480.94 490 490.94 490 490.94 480 490.94 470"
			/>
			<polygon
				fill={color3}
				points="430.94 310 420.94 310 420.94 300 420.94 290 410.94 290 410.94 300 410.94 310 400.94 310 400.94 320 410.94 320 420.94 320 420.94 330 430.94 330 430.94 340 440.94 340 440.94 330 440.94 320 430.94 320 430.94 310"
			/>
			<rect fill={color3} x="410.94" y="550" width="10" height="10" />
			<rect fill={color3} x="410.94" y="510" width="10" height="10" />
			<rect fill={color3} x="410.94" y="470" width="10" height="10" />
			<rect fill={color3} x="410.94" y="430" width="10" height="10" />
			<rect fill={color3} x="410.94" y="390" width="10" height="10" />
			<rect fill={color3} x="410.94" y="350" width="10" height="10" />
			<polygon
				fill={color3}
				points="390.94 310 380.94 310 380.94 300 370.94 300 370.94 290 370.94 280 370.94 270 360.94 270 360.94 280 360.94 290 350.94 290 350.94 300 340.94 300 330.94 300 330.94 310 340.94 310 350.94 310 360.94 310 370.94 310 370.94 320 380.94 320 380.94 330 390.94 330 390.94 340 400.94 340 400.94 330 400.94 320 390.94 320 390.94 310"
			/>
			<polygon
				fill={color3}
				points="330.94 400 320.94 400 320.94 410 320.94 420 320.94 430 320.94 440 320.94 450 320.94 460 320.94 470 320.94 480 320.94 490 320.94 500 320.94 510 320.94 520 320.94 530 320.94 540 310.94 540 310.94 550 310.94 560 310.94 570 310.94 580 320.94 580 330.94 580 330.94 570 330.94 560 330.94 550 330.94 540 330.94 530 340.94 530 340.94 520 340.94 510 340.94 500 340.94 490 340.94 480 340.94 470 340.94 460 330.94 460 330.94 450 330.94 440 330.94 430 340.94 430 340.94 420 340.94 410 330.94 410 330.94 400"
			/>
			<polygon
				fill={color3}
				points="310.94 310 310.94 320 320.94 320 330.94 320 330.94 310 320.94 310 310.94 310"
			/>
			<polygon
				fill={color3}
				points="300.94 330 290.94 330 290.94 340 280.94 340 280.94 350 280.94 360 270.94 360 270.94 370 270.94 380 270.94 390 270.94 400 270.94 410 270.94 420 270.94 430 270.94 440 270.94 450 270.94 460 270.94 470 270.94 480 270.94 490 270.94 500 270.94 510 270.94 520 270.94 530 270.94 540 270.94 550 270.94 560 270.94 570 270.94 580 280.94 580 280.94 570 280.94 560 280.94 550 280.94 540 280.94 530 280.94 520 280.94 510 280.94 500 280.94 490 280.94 480 280.94 470 280.94 460 280.94 450 280.94 440 280.94 430 280.94 420 280.94 410 280.94 400 280.94 390 280.94 380 280.94 370 290.94 370 290.94 360 290.94 350 300.94 350 300.94 340 310.94 340 310.94 330 310.94 320 300.94 320 300.94 330"
			/>
		</g>
	);
};

export default Shirt;