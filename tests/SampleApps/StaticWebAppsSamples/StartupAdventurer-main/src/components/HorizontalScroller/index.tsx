import React from "react";
import { ScrollContainer } from "./styles";

interface IProps {
  children: React.ReactChildren | React.ReactChild | any;
}

const HorizontalScroller = ({ children }: IProps) => {
  return (
    <div style={{ overflowX: "scroll", overflowY: "hidden" }}>
      <ScrollContainer>{children}</ScrollContainer>
    </div>
  );
};

export default HorizontalScroller;
