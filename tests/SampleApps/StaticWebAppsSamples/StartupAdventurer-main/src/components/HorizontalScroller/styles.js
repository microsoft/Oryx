import styled from "styled-components";

export const ScrollContainer = styled.div.attrs(() => ({
  className: "scroller horizontal-scroller",
}))`
  display: flex;
  flex-wrap: nowrap;
  padding-left: 49px;
  -webkit-overflow-scrolling: touch;
  white-space: nowrap;
  width: 100%;

  ::-webkit-scrollbar {
    display: none;
  }

  &::after {
    content: "";
    display: block;
    flex: 0 0 20px;
    width: 20px;
  }
`;
