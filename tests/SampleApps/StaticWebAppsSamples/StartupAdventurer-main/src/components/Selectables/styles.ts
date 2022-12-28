import styled, { css } from "styled-components";
import { colors } from "@/utils/style-utils";
import clsx from "clsx";

interface IWrapperProps {
  isHorizontal?: boolean;
  hasThumbnail?: boolean;
}

export const Wrapper = styled.div.attrs((props: IWrapperProps) => ({
  className: clsx("options-wrapper", props.isHorizontal && "options-wrapper-horizontal"),
}))<IWrapperProps>`
  padding: 0 49px;
  margin-bottom: 114px;

  &:last-of-type {
    margin-bottom: 0;
  }

  &:focus-within {
    background-color: ${colors.lightGrey};
  }

  ${(props) =>
    props.isHorizontal &&
    css`
      padding: 0;

      .options-header {
        padding: 0 49px;
      }
    `}
`;

export const Header = styled.div.attrs(() => ({
  className: "options-header",
}))`
  align-items: center;
  display: flex;
  justify-content: space-between;
  margin-bottom: 25px;
`;

export const Title = styled.h2`
  font-size: 50px;
  font-weight: normal;
  line-height: 1;
  margin: 0;
`;

export const Deselector = styled.button`
  background: rgba(255, 255, 255, 0);
  border: 0;
  font-size: 50px;
  padding: 0 50px 0 0;
  position: relative;

  &[disabled] {
    color: #d6d6d6;
    pointer-events: none;

    &::after,
    &::before {
      background: #d6d6d6;
    }
  }

  &::after,
  &::before {
    background: #000;
    content: "";
    display: block;
    height: 7px;
    width: 35px;
    position: absolute;
    top: 50%;
    right: 0;
  }

  &::after {
    transform: translate(0, -50%) rotate(45deg);
  }

  &::before {
    transform: translate(0, -50%) rotate(-45deg);
  }
`;

export const Container = styled.div.attrs(() => ({
  className: "options-container",
}))`
  display: flex;
`;

interface IThumbnailProps {
  active?: boolean;
}

export const Thumbnail = styled.button.attrs(() => ({ className: "style-thumbnail" }))<IThumbnailProps>`
  background: ${colors.offWhite};
  border: 7px solid ${(props) => (props.active ? colors.black : colors.lightGrey)};
  border-radius: 0;
  cursor: pointer;
  flex-shrink: 0;
  height: 256px;
  margin-right: 28px;
  margin: 7px 28px 7px 0;
  padding: 30px;
  width: 256px;

  &:focus {
    outline: 7px solid ${colors.green};
  }

  svg {
    height: 100%;
    width: 100%;
  }
`;

export const Swatches = styled.div`
  align-content: space-between;
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
`;

interface IColorSwatchProps {
  active?: boolean;
}

export const ColorSwatch = styled.button<IColorSwatchProps>`
  align-items: center;
  background: ${colors.offWhite};
  border: 7px solid ${(props) => (props.active ? colors.black : colors.lightGrey)};
  border-radius: 0;
  display: flex;
  flex-basis: 256px;
  height: 109px;
  margin: 0;
  padding: 0 30px;
  justify-content: center;
  width: 256px;

  .options-wrapper-horizontal & {
    margin-right: 28px;
    flex-shrink: 0;
  }
`;

export const Color = styled.span`
  background-color: ${(props) => props.color || "transparent"};
  flex: 1 0 auto;
  height: 52px;

  + span {
    margin-left: -1px;
  }
`;
