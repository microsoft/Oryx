import styled from "styled-components";
import { colors, buttonShine } from "@/utils/style-utils";

export const StartContainer = styled.div`
  align-items: center;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  opacity: 0;
  padding: 300px 0;
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
`;

export const StartChapters = styled.div`
  margin: 0 auto 190px;
  max-width: 3045px;

  p {
    color: ${colors.white};
    font-size: 100px;
    line-height: 1.1;
    text-align: center;

    span {
      opacity: 0;
    }

    &:first-of-type {
      span:nth-of-type(-n + 5) {
        color: ${colors.green};
      }
    }
    &:last-of-type {
      span:nth-of-type(-n + 6) {
        color: ${colors.green};
      }
    }
  }
`;

export const StartButton = styled.button`
  align-items: center;
  background-color: ${colors.white};
  border: 7px solid ${colors.lightGrey};
  border-radius: 0;
  box-shadow: 0px 7px 0px ${colors.darkBlue};
  font-size: 60px;
  display: flex;
  height: 176px;
  margin: 0 0 200px;
  opacity: 0;
  padding: 0 80px;

  ${buttonShine}

  &:focus {
    border: none;
    outline: 10px solid ${colors.green};
  }

  &:active {
    background-color: ${colors.lightGrey};
  }

  a {
    text-decoration: none;

    color: #000;

    &:visited {
      color: #000;
    }
  }
`;
