
#include <gl/glut.h>
#include <windows.h>

#pragma comment(linker,"/subsystem:windows /ENTRY:mainCRTStartup")

void render_loop()
{
	glClearColor(0, 0, 0, 0);
	glBegin(GL_POLYGON);
	glVertex2d(0, 0);
	glVertex2d(5, 5);
	glVertex2d(10, 10);
	glEnd();

	clock();

	glFlush();
}

void render_init(int argc, char** argv)
{
	glutInit(&argc, argv);

	int widthSys = GetSystemMetrics(SM_CXSCREEN);
	int heightSys = GetSystemMetrics(SM_CYSCREEN);

	int widthApp = 960;
	int heightApp = 540;

	int posx = widthSys * 0.5 - widthApp * 0.5;
	int posy = heightSys * 0.5 - heightApp * 0.5;

	glutInitWindowSize(widthApp, heightApp);
	glutInitWindowPosition(posx, posy);
	glutCreateWindow("Unity");

	glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGBA | GLUT_DEPTH | GLUT_STENCIL | GLUT_ALPHA);
	glutDisplayFunc(render_loop);

	const GLubyte* version = glGetString(GL_VERSION);

	glutMainLoop();
}

void server_loop()
{

}

void server_init(int argc, char** argv)
{
	//WORD wd = MAKEWORD(2, 2);
	//LPWSADATA lpwsaData = NULL;
	//if (WSAStartup(wd, lpwsaData) != 0)
	//{
	//	return;
	//}
	//hostent* host = gethostbyname("www.baidu.com");
	//int x = 0;
}

int main(int argc, char** argv)
{
	render_init(argc,argv);
	server_init(argc, argv);
}